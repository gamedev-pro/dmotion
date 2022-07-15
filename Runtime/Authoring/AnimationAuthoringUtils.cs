using System;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DOTSAnimation
{
    public struct BufferElementRef<T> where T : struct, IBufferElementData
    {
        public Entity Entity;
        public EntityManager EntityManager;
        public int Index;

        public DynamicBuffer<T> Buffer => EntityManager.GetBuffer<T>(Entity);

        public T Value
        {
            get
            {
                var buffer = Buffer;
                if (buffer.IsCreated)
                {
                    return buffer.ElementAtSafe(Index);
                }

                return default;
            }
            set
            {
                var buffer = Buffer;
                if (buffer.IsCreated)
                {
                    buffer[Index] = value;
                }
            }
        }

        public bool IsValid
        {
            get
            {
                var buffer = Buffer;
                return buffer.IsCreated && buffer.IsValidIndex(Index);
            }
        }
    }

    public struct ClipSamplerCreateParams
    {
        public float Speed;
        public float Threshold;

        public ClipSamplerCreateParams(float speed, float threshold)
        {
            Speed = speed;
            Threshold = threshold;
        }

        public static ClipSamplerCreateParams Default = new ClipSamplerCreateParams()
        {
            Speed = 1,
            Threshold = 0
        };
    }
    public struct AnimationStateCreateParams
    {
        private const float DefaultTransition = 0.15f;
        public bool Loop;
        public float TransitionDuration;
        public bool IsOneShot;
        public AnimationSamplerType SamplerType;

        public AnimationStateCreateParams(AnimationSamplerType samplerType, float transitionDuration = 0.15f,
            bool loop = true, bool isOneShot = false)
        {
            Loop = loop;
            TransitionDuration = transitionDuration;
            IsOneShot = isOneShot;
            SamplerType = samplerType;
        }

        public static AnimationStateCreateParams DefaultNonLoop = new AnimationStateCreateParams()
        {
            Loop = false,
            TransitionDuration = DefaultTransition,
            IsOneShot = false,
            SamplerType = AnimationSamplerType.Single
        };
        public static AnimationStateCreateParams DefaultLoop = new AnimationStateCreateParams()
        {
            Loop = true,
            TransitionDuration = DefaultTransition,
            IsOneShot = false,
            SamplerType = AnimationSamplerType.Single
        };
        
        public static AnimationStateCreateParams OneShot = new AnimationStateCreateParams()
        {
            Loop = false,
            TransitionDuration = DefaultTransition,
            IsOneShot = true,
            SamplerType = AnimationSamplerType.Single
        };
    }
    public static class AnimationAuthoringUtils
    {
        public static SmartBlobberHandle<SkeletonClipSetBlob> RequestBlobAssets(UnityEngine.AnimationClip[] clips,
            GameObject gameObject, GameObjectConversionSystem conversionSystem)
        {
            var clipConfigs = new SkeletonClipConfig[clips.Length];
            for (int i = 0; i < clipConfigs.Length; i++)
            {
                clipConfigs[i] = new SkeletonClipConfig()
                {
                    clip = clips[i],
                    settings = SkeletonClipCompressionSettings.kDefaultSettings
                };
            }

            return conversionSystem.CreateBlob(gameObject, new SkeletonClipSetBakeData()
            {
                animator = gameObject.GetComponent<Animator>(),
                clips = clipConfigs
            });
        }

        public static void ForEachBone(Entity skeletonEntity, EntityManager dstManager, System.Action<Entity> action)
        {
            if (dstManager.HasComponent<BoneReference>(skeletonEntity))
            {
                var bones = dstManager.GetBuffer<BoneReference>(skeletonEntity, true).ToNativeArray(Allocator.Temp);
                foreach (var b in bones)
                {
                    action(b.bone);
                }
            }
            else
            {
                action(skeletonEntity);
            }
        }

        public static void AddAnimationStateMachine(Entity e, EntityManager dstManager,
            GameObjectConversionSystem conversionSystem, GameObject owner, BufferElementRef<AnimationState> startState)
        {
            dstManager.AddComponentData(e, new AnimationStateMachine()
            {
                CurrentState = new AnimationStateMachine.StateRef(){StateIndex = startState.Index},
                NextState = AnimationStateMachine.StateRef.Null,
                PrevState = AnimationStateMachine.StateRef.Null,
                RequestedNextState = AnimationStateMachine.StateRef.Null
            });

            var ownerEntity = conversionSystem.GetPrimaryEntity(owner);
            dstManager.AddComponentData(e, new AnimatorEntity() { Owner = ownerEntity });
            dstManager.AddComponentData(ownerEntity, new AnimatorOwner() { AnimatorEntity = e });
            dstManager.AddComponent<TransferRootMotion>(ownerEntity);

            dstManager.AddComponent<RootDeltaPosition>(e);
            dstManager.AddComponent<RootDeltaRotation>(e);
            //Add all required components for fms system to run
            dstManager.GetOrCreateBuffer<AnimationEvent>(e);
            dstManager.GetOrCreateBuffer<BoolTransition>(e);
            dstManager.GetOrCreateBuffer<BoolParameter>(e);
            dstManager.GetOrCreateBuffer<ExitTimeTransition>(e);
        }

        public static BufferElementRef<ClipSampler> AddSampler(Entity e, EntityManager dstManager,
            BlobAssetReference<SkeletonClipSetBlob> clipsBlob, int clipIndex, ClipSamplerCreateParams createParams)
        {
            var sampler = new ClipSampler()
            {
                Blob = clipsBlob,
                ClipIndex = clipIndex,
                Threshold = createParams.Threshold,
                Speed = createParams.Speed
            };

            var samplersBuffer = dstManager.GetOrCreateBuffer<ClipSampler>(e);
            samplersBuffer.Add(sampler);

            return new BufferElementRef<ClipSampler>()
            {
                Entity = e,
                EntityManager = dstManager,
                Index = samplersBuffer.Length - 1
            };
        }
        
        public static BufferElementRef<AnimationState> AddAnimationState(
            Entity e, EntityManager dstManager,
            int startSamplerIndex, int endSamplerIndex, AnimationStateCreateParams createParams, string stateName = "")
        {
            var state = new AnimationState()
            {
                StartSamplerIndex = startSamplerIndex,
                EndSamplerIndex = endSamplerIndex,
                Loop = createParams.Loop,
                TransitionDuration = createParams.TransitionDuration,
                Type = createParams.SamplerType
            };

            var stateBuffer = dstManager.GetOrCreateBuffer<AnimationState>(e);
            stateBuffer.Add(state);

            var stateRef = new BufferElementRef<AnimationState>()
            {
                Entity = e,
                EntityManager = dstManager,
                Index = stateBuffer.Length - 1
            };

            if (createParams.IsOneShot)
            {
                AddStateMachineReturnTransition(stateRef);
            }

            return stateRef;
        }
        public static BufferElementRef<AnimationState> AddAnimationState(
            in BufferElementRef<ClipSampler> sampler, AnimationStateCreateParams createParams, string stateName = "")
        {
            return AddAnimationState(sampler.Entity, sampler.EntityManager, sampler.Index, sampler.Index, createParams, stateName);
        }
        
        public static BufferElementRef<AnimationState> AddAnimationState_Single(
            Entity e, EntityManager dstManager,
            BlobAssetReference<SkeletonClipSetBlob> clipsBlob, int clipIndex, ClipSamplerCreateParams samplerCreateParams, AnimationStateCreateParams stateCreateParams, string stateName = "")
        {
            var sampler = AddSampler(e, dstManager, clipsBlob, clipIndex, samplerCreateParams);
            return AddAnimationState(sampler, stateCreateParams, stateName);
        }


        public static BufferElementRef<AnimationEvent> AddEvent(this BufferElementRef<AnimationState> stateRef,
            float normalizedTime, int eventHash)
        {
            var state = stateRef.Value;
            Assert.IsTrue(state.Type == AnimationSamplerType.Single);
            Assert.IsTrue(state.StartSamplerIndex == state.EndSamplerIndex);
            var sampler = new BufferElementRef<ClipSampler>()
            {
                Entity = stateRef.Entity,
                EntityManager = stateRef.EntityManager,
                Index = state.StartSamplerIndex
            };
            return AddEvent(sampler, normalizedTime, eventHash);
        }
        
        public static BufferElementRef<AnimationEvent> AddEvent(this BufferElementRef<ClipSampler> samplerRef, float normalizedTime, int eventHash)
        {
            var eventsBuffer = samplerRef.EntityManager.GetOrCreateBuffer<AnimationEvent>(samplerRef.Entity);

            var newEvent = new AnimationEvent()
            {
                SamplerIndex = samplerRef.Index,
                EventHash = eventHash,
                NormalizedTime = normalizedTime
            };
            eventsBuffer.Add(newEvent);

            return new BufferElementRef<AnimationEvent>()
            {
                Entity = samplerRef.Entity,
                EntityManager = samplerRef.EntityManager,
                Index = eventsBuffer.Length - 1
            };
        }

        public static BufferElementRef<BoolParameter> AddParameter<T>(Entity e, EntityManager dstManager,
            FixedString32Bytes name)
            where T : struct, IComparable<T>
        {
            var parameterBuffer = dstManager.GetOrCreateBuffer<BoolParameter>(e);
            parameterBuffer.Add(new BoolParameter()
            {
                Hash = name.GetHashCode(),
            });
            return new BufferElementRef<BoolParameter>()
            {
                Entity = e,
                EntityManager = dstManager,
                Index = parameterBuffer.Length - 1
            };
        }

        public static BufferElementRef<AnimationTransitionGroup> AddTransitionGroup(
            BufferElementRef<AnimationState> stateRef, BufferElementRef<AnimationState> toStateRef)
        {
            var transitions = stateRef.EntityManager.GetOrCreateBuffer<AnimationTransitionGroup>(stateRef.Entity);

            var transition = new AnimationTransitionGroup()
            {
                FromStateIndex = stateRef.Index,
                ToStateIndex = toStateRef.Index,
            };

            transitions.Add(transition);

            return new BufferElementRef<AnimationTransitionGroup>()
            {
                Entity = stateRef.Entity,
                EntityManager = stateRef.EntityManager,
                Index = transitions.Length - 1
            };
        }


        public static BufferElementRef<BoolTransition> AddSingleTransition(
            BufferElementRef<AnimationState> stateRef, BufferElementRef<AnimationState> toStateRef,
            BufferElementRef<BoolParameter> parameterRef, bool comparisonValue)
        {
            var group = AddTransitionGroup(stateRef, toStateRef);
            return AddTransition(group, parameterRef, comparisonValue);
        }

        public static BufferElementRef<BoolTransition> AddTransition(
            BufferElementRef<AnimationTransitionGroup> groupRef,
            BufferElementRef<BoolParameter> parameterRef, bool comparisonValue)
        {
            var transitions = groupRef.EntityManager.GetOrCreateBuffer<BoolTransition>(groupRef.Entity);

            var transition = new BoolTransition() 
            {
                GroupIndex = groupRef.Index,
                ParameterIndex = parameterRef.Index,
                ComparisonValue = comparisonValue,
            };

            transitions.Add(transition);

            return new BufferElementRef<BoolTransition>()
            {
                Entity = groupRef.Entity,
                EntityManager = groupRef.EntityManager,
                Index = transitions.Length - 1
            };
        }
        
        public static BufferElementRef<BlendParameter> AddBlendParameter(BufferElementRef<AnimationState> stateRef, FixedString32Bytes name)
        {
            var blendParameters = stateRef.EntityManager.GetOrCreateBuffer<BlendParameter>(stateRef.Entity);

            var blend = new BlendParameter()
            {
                StateIndex = stateRef.Index,
                Hash = name.GetHashCode()
            };

            blendParameters.Add(blend);

            return new BufferElementRef<BlendParameter>()
            {
                Entity = stateRef.Entity,
                EntityManager = stateRef.EntityManager,
                Index = blendParameters.Length - 1
            };
        }
        
        public static BufferElementRef<ExitTimeTransition> AddStateMachineReturnTransition(in BufferElementRef<AnimationState> stateRef, float normalizedExitTime = 0.95f)
        {
            var transitions = stateRef.EntityManager.GetOrCreateBuffer<ExitTimeTransition>(stateRef.Entity);

            var transition = new ExitTimeTransition()
            {
                FromStateIndex = stateRef.Index,
                ToStateIndex = -1,
                NormalizedExitTime = normalizedExitTime
            };

            transitions.Add(transition);

            return new BufferElementRef<ExitTimeTransition>()
            {
                Entity = stateRef.Entity,
                EntityManager = stateRef.EntityManager,
                Index = transitions.Length - 1
            };
        }
        public static void Remove<T>(this BufferElementRef<T> bufferRef)
            where T : struct, IBufferElementData
        {
            var buffer = bufferRef.Buffer;
            buffer.RemoveAt(bufferRef.Index);
            
            bufferRef.Entity = Entity.Null;
            bufferRef.EntityManager = default;
            bufferRef.Index = -1;
        }

        public static AnimationStateRef ToStateRef(this BufferElementRef<AnimationState> state)
        {
            return new AnimationStateRef() { Index = state.Index };
        }
    }
}