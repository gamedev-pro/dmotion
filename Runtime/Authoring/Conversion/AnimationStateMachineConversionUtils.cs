using System;
using System.Linq;
using Latios.Kinemation;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public static class AnimationStateMachineConversionUtils
    {
        public static BlobAssetReference<StateMachineBlob> CreateStateMachineBlob(StateMachineAsset stateMachineAsset)
        {
            return CreateConverter(stateMachineAsset).BuildBlob();
        }

        internal static StateMachineBlobConverter CreateConverter(StateMachineAsset stateMachineAsset)
        {
            var converter = new StateMachineBlobConverter();
            var defaultStateIndex = stateMachineAsset.States.ToList().IndexOf(stateMachineAsset.DefaultState);
            Assert.IsTrue(defaultStateIndex >= 0,
                $"Couldn't find state {stateMachineAsset.DefaultState.name}, in state machine {stateMachineAsset.name}");
            converter.DefaultStateIndex = (byte)defaultStateIndex;
            BuildStates(stateMachineAsset, ref converter, Allocator.Persistent);
            return converter;
        }

        private static void BuildStates(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter,
            Allocator allocator)
        {
            var singleClipStates = stateMachineAsset.States.OfType<SingleClipStateAsset>().ToArray();
            var linearBlendStates = stateMachineAsset.States.OfType<LinearBlendStateAsset>().ToArray();

            converter.States =
                new UnsafeList<AnimationStateConversionData>(stateMachineAsset.States.Count, allocator);
            converter.States.Resize(stateMachineAsset.States.Count);

            converter.SingleClipStates =
                new UnsafeList<SingleClipStateBlob>(singleClipStates.Length, allocator);

            converter.LinearBlendStates =
                new UnsafeList<LinearBlendStateConversionData>(linearBlendStates.Length,
                    allocator);

            ushort clipIndex = 0;
            for (var i = 0; i < converter.States.Length; i++)
            {
                var stateAsset = stateMachineAsset.States[i];
                var stateImplIndex = -1;
                switch (stateAsset)
                {
                    case LinearBlendStateAsset linearBlendStateAsset:
                        stateImplIndex = converter.LinearBlendStates.Length;
                        var blendParameterIndex =
                            stateMachineAsset.Parameters
                                .OfType<FloatParameterAsset>()
                                .ToList()
                                .FindIndex(f => f == linearBlendStateAsset.BlendParameter);

                        Assert.IsTrue(blendParameterIndex >= 0,
                            $"({stateMachineAsset.name}) Couldn't find parameter {linearBlendStateAsset.BlendParameter.name}, for Linear Blend State");

                        var linearBlendState = new LinearBlendStateConversionData()
                        {
                            BlendParameterIndex = (ushort)blendParameterIndex
                        };

                        linearBlendState.ClipsWithThresholds = new UnsafeList<ClipIndexWithThreshold>(
                            linearBlendStateAsset.BlendClips.Length, allocator);

                        linearBlendState.ClipsWithThresholds.Resize(linearBlendStateAsset.BlendClips.Length);
                        for (ushort blendClipIndex = 0;
                             blendClipIndex < linearBlendState.ClipsWithThresholds.Length;
                             blendClipIndex++)
                        {
                            linearBlendState.ClipsWithThresholds[blendClipIndex] = new ClipIndexWithThreshold
                            {
                                ClipIndex = clipIndex,
                                Threshold = linearBlendStateAsset.BlendClips[blendClipIndex].Threshold,
                                Speed = linearBlendStateAsset.BlendClips[blendClipIndex].Speed
                            };
                            clipIndex++;
                        }

                        converter.LinearBlendStates.Add(linearBlendState);
                        break;
                    case SingleClipStateAsset singleClipStateAsset:
                        stateImplIndex = converter.SingleClipStates.Length;
                        converter.SingleClipStates.Add(new SingleClipStateBlob()
                        {
                            ClipIndex = clipIndex,
                        });
                        clipIndex++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(stateAsset));
                }

                Assert.IsTrue(stateImplIndex >= 0, $"Index to state implementation needs to be assigned");
                converter.States[i] =
                    BuildStateConversionData(stateMachineAsset, stateAsset, stateImplIndex, allocator);
            }
        }

        private static AnimationStateConversionData BuildStateConversionData(StateMachineAsset stateMachineAsset,
            AnimationStateAsset state, int stateIndex, Allocator allocator)
        {
            var stateConversionData = new AnimationStateConversionData()
            {
                Type = state.Type,
                StateIndex = (ushort)stateIndex,
                Loop = state.Loop,
                Speed = state.Speed
            };

            //Create Transition Groups
            var transitionCount = state.OutTransitions.Count;
            stateConversionData.Transitions =
                new UnsafeList<StateOutTransitionConversionData>(transitionCount, allocator);
            stateConversionData.Transitions.Resize(transitionCount);

            for (var transitionIndex = 0; transitionIndex < stateConversionData.Transitions.Length; transitionIndex++)
            {
                var outTransitionAsset = state.OutTransitions[transitionIndex];

                var toStateIndex =
                    (short)stateMachineAsset.States.ToList().FindIndex(s => s == outTransitionAsset.ToState);
                Assert.IsTrue(toStateIndex >= 0,
                    $"State {outTransitionAsset.ToState.name} not present on State Machine {stateMachineAsset.name}");
                var outTransition = new StateOutTransitionConversionData()
                {
                    ToStateIndex = toStateIndex,
                    TransitionEndTime = outTransitionAsset.HasEndTime ? Mathf.Max(0, outTransitionAsset.EndTime) : -1f,
                    TransitionDuration = outTransitionAsset.TransitionDuration,
                };

                //Create bool transitions
                {
                    var boolTransitions = outTransitionAsset.BoolTransitions.ToArray();
                    outTransition.BoolTransitions =
                        new UnsafeList<BoolTransition>(boolTransitions.Length, allocator);
                    outTransition.BoolTransitions.Resize(boolTransitions.Length);
                    for (var boolTransitionIndex = 0;
                         boolTransitionIndex < outTransition.BoolTransitions.Length;
                         boolTransitionIndex++)
                    {
                        var boolTransitionAsset = boolTransitions[boolTransitionIndex];
                        var parameterIndex = stateMachineAsset.Parameters
                            .OfType<BoolParameterAsset>()
                            .ToList()
                            .FindIndex(p => p == boolTransitionAsset.BoolParameter);

                        Assert.IsTrue(parameterIndex >= 0,
                            $"({stateMachineAsset.name}) Couldn't find parameter {boolTransitionAsset.BoolParameter.name}, for transition");
                        outTransition.BoolTransitions[boolTransitionIndex] = new BoolTransition
                        {
                            ComparisonValue = boolTransitionAsset.ComparisonValue == BoolConditionComparison.True,
                            ParameterIndex = parameterIndex
                        };
                    }
                }

                //Create int transitions
                {
                    var intTransitions = outTransitionAsset.IntTransitions.ToArray();
                    var intParameters = stateMachineAsset.Parameters
                        .OfType<IntParameterAsset>()
                        .ToList();
                    outTransition.IntTransitions =
                        new UnsafeList<IntTransition>(intTransitions.Length, allocator);
                    outTransition.IntTransitions.Resize(intTransitions.Length);
                    for (var intTransitionIndex = 0;
                         intTransitionIndex < outTransition.IntTransitions.Length;
                         intTransitionIndex++)
                    {
                        var intTransitionAsset = intTransitions[intTransitionIndex];
                        var parameterIndex = intParameters.FindIndex(p => p == intTransitionAsset.IntParameter);

                        Assert.IsTrue(parameterIndex >= 0,
                            $"({stateMachineAsset.name}) Couldn't find parameter {intTransitionAsset.IntParameter.name}, for transition");
                        outTransition.IntTransitions[intTransitionIndex] = new IntTransition
                        {
                            ParameterIndex = parameterIndex,
                            ComparisonValue = intTransitionAsset.ComparisonValue,
                            ComparisonMode = intTransitionAsset.ComparisonMode
                        };
                    }
                }

                stateConversionData.Transitions[transitionIndex] = outTransition;
            }

            return stateConversionData;
        }

        internal static void AddAnimationStateSystemComponents(EntityCommands dstManager, Entity entity)
        {
            dstManager.AddBuffer<AnimationState>(entity);
            dstManager.AddComponent(entity, AnimationStateTransition.Null);
            dstManager.AddComponent(entity, AnimationStateTransitionRequest.Null);
            dstManager.AddComponent(entity, AnimationCurrentState.Null);
            dstManager.AddComponent(entity, AnimationPreserveState.Null);
            var clipSamplers = dstManager.AddBuffer<ClipSampler>(entity);
            clipSamplers.Capacity = 10;
        }

        public static void AddOneShotSystemComponents(EntityCommands dstManager, Entity entity)
        {
            dstManager.AddComponent(entity, PlayOneShotRequest.Null);
            dstManager.AddComponent(entity, OneShotState.Null);
        }

        internal static void AddStateMachineParameters(IBaker dstManager, Entity entity,
            StateMachineAsset stateMachineAsset)
        {
            //Parameters
            {
                var boolParameters = dstManager.AddBuffer<BoolParameter>(entity);
                var intParameters = dstManager.AddBuffer<IntParameter>(entity);
                var floatParameters = dstManager.AddBuffer<FloatParameter>(entity);
                foreach (var p in stateMachineAsset.Parameters)
                {
                    switch (p)
                    {
                        case BoolParameterAsset:
                            boolParameters.Add(new BoolParameter(p.Hash));
                            break;
                        case IntParameterAsset:
                            intParameters.Add(new IntParameter(p.Hash));
                            break;
                        case FloatParameterAsset:
                            floatParameters.Add(new FloatParameter(p.Hash));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(p));
                    }
                }
            }

#if UNITY_EDITOR || DEBUG
            dstManager.AddComponentObject(entity, new AnimationStateMachineDebug
            {
                StateMachineAsset = stateMachineAsset
            });
#endif
        }


        internal static void AddStateMachineParameters(EntityCommands dstManager, Entity entity,
            StateMachineAsset stateMachineAsset)
        {
            //Parameters
            {
                dstManager.AddBuffer<BoolParameter>(entity);
                dstManager.AddBuffer<IntParameter>(entity);
                dstManager.AddBuffer<FloatParameter>(entity);

                var boolParameters = dstManager.GetBuffer<BoolParameter>(entity);
                var intParameters = dstManager.GetBuffer<IntParameter>(entity);
                var floatParameters = dstManager.GetBuffer<FloatParameter>(entity);

                foreach (var p in stateMachineAsset.Parameters)
                {
                    switch (p)
                    {
                        case BoolParameterAsset:
                            boolParameters.Add(new BoolParameter(p.Hash));
                            break;
                        case IntParameterAsset:
                            intParameters.Add(new IntParameter(p.Hash));
                            break;
                        case FloatParameterAsset:
                            floatParameters.Add(new FloatParameter(p.Hash));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(p));
                    }
                }
            }

#if UNITY_EDITOR || DEBUG
            dstManager.AddComponentObject(entity, new AnimationStateMachineDebug
            {
                StateMachineAsset = stateMachineAsset
            });
#endif
        }

        internal static void AddStateMachineSystemComponents(EntityCommands dstManager, Entity entity,
            StateMachineAsset stateMachineAsset,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clipsBlob,
            BlobAssetReference<ClipEventsBlob> clipEventsBlob)
        {
            //state machine data
            {
                var stateMachine = new AnimationStateMachine
                {
                    StateMachineBlob = stateMachineBlob,
                    ClipsBlob = clipsBlob,
                    ClipEventsBlob = clipEventsBlob,
                    CurrentState = StateMachineStateRef.Null
                };

                dstManager.AddComponent(entity, stateMachine);

                dstManager.AddBuffer<SingleClipState>(entity);
                dstManager.AddBuffer<LinearBlendStateMachineState>(entity);
            }
            AddStateMachineParameters(dstManager, entity, stateMachineAsset);
        }

        internal static void AddStateMachineSystemComponents(EntityCommands dstManager, Entity entity,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clipsBlob,
            BlobAssetReference<ClipEventsBlob> clipEventsBlob)
        {
            //state machine data
            {
                var stateMachine = new AnimationStateMachine
                {
                    StateMachineBlob = stateMachineBlob,
                    ClipsBlob = clipsBlob,
                    ClipEventsBlob = clipEventsBlob,
                    CurrentState = StateMachineStateRef.Null
                };

                dstManager.AddComponent(entity, stateMachine);

                dstManager.AddBuffer<SingleClipState>(entity);
                dstManager.AddBuffer<LinearBlendStateMachineState>(entity);
            }
        }

        public static void AddSingleClipStateComponents(EntityCommands dstManager, Entity ownerEntity, Entity entity,
            bool enableEvents = true, bool enableSingleClipRequest = true,
            RootMotionMode rootMotionMode = RootMotionMode.Disabled)
        {
            AnimationStateMachineConversionUtils.AddAnimationStateSystemComponents(dstManager, entity);

            dstManager.AddBuffer<SingleClipState>(entity);

            if (enableEvents)
            {
                dstManager.AddBuffer<RaisedAnimationEvent>(entity);
            }

            if (enableSingleClipRequest)
            {
                dstManager.AddComponent(entity, PlaySingleClipRequest.Null);
            }

            if (ownerEntity != entity)
            {
                AnimationStateMachineConversionUtils.AddAnimatorOwnerComponents(dstManager, ownerEntity, entity);
            }

            AnimationStateMachineConversionUtils.AddRootMotionComponents(dstManager, ownerEntity, entity,
                rootMotionMode);
        }

        public static void AddAnimatorOwnerComponents(EntityCommands dstManager, Entity ownerEntity, Entity entity)
        {
            dstManager.AddComponent(ownerEntity, new AnimatorOwner { AnimatorEntity = entity });
            dstManager.AddComponent(entity, new AnimatorEntity { Owner = ownerEntity });
        }

        public static void AddRootMotionComponents(EntityCommands dstManager, Entity ownerEntity, Entity entity,
            RootMotionMode rootMotionMode)
        {
            switch (rootMotionMode)
            {
                case RootMotionMode.Disabled:
                    break;
                case RootMotionMode.EnabledAutomatic:
                    dstManager.AddComponent(entity, new RootDeltaTranslation());
                    dstManager.AddComponent(entity, new RootDeltaRotation());
                    if (ownerEntity != entity)
                    {
                        dstManager.AddComponent(ownerEntity, new TransferRootMotionToOwner());
                    }
                    else
                    {
                        dstManager.AddComponent(entity, new ApplyRootMotionToEntity());
                    }

                    break;
                case RootMotionMode.EnabledManual:
                    dstManager.AddComponent(entity, new RootDeltaTranslation());
                    dstManager.AddComponent(entity, new RootDeltaRotation());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}