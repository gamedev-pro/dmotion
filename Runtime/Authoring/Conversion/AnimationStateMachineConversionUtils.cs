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
    internal static class AnimationStateMachineConversionUtils
    {
        public static BlobAssetReference<StateMachineBlob> CreateStateMachineBlob(StateMachineAsset stateMachineAsset,
            Allocator allocator)
        {
            return CreateConverter(stateMachineAsset, allocator).BuildBlob();
        }

        internal static StateMachineBlobConverter CreateConverter(StateMachineAsset stateMachineAsset,
            Allocator allocator)
        {
            var converter = new StateMachineBlobConverter();
            var defaultStateIndex = stateMachineAsset.States.ToList().IndexOf(stateMachineAsset.DefaultState);
            Assert.IsTrue(defaultStateIndex >= 0,
                $"Couldn't find state {stateMachineAsset.DefaultState.name}, in state machine {stateMachineAsset.name}");
            converter.DefaultStateIndex = (byte)defaultStateIndex;
            BuildStates(stateMachineAsset, ref converter, allocator);
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
                                Threshold = linearBlendStateAsset.BlendClips[blendClipIndex].Threshold
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

        internal static void AddAnimationStateSystemComponents(EntityManager dstManager, Entity entity)
        {
            dstManager.AddBuffer<AnimationState>(entity);
            dstManager.AddComponentData(entity, AnimationStateTransition.Null);
            dstManager.AddComponentData(entity, AnimationStateTransitionRequest.Null);
            dstManager.AddComponentData(entity, AnimationCurrentState.Null);
            var clipSamplers = dstManager.AddBuffer<ClipSampler>(entity);
            clipSamplers.Capacity = 10;
        }

        internal static void AddOneShotSystemComponents(EntityManager dstManager, Entity entity)
        {
            dstManager.AddComponentData(entity, PlayOneShotRequest.Null);
            dstManager.AddComponentData(entity, OneShotState.Null);
        }

        internal static void AddStateMachineSystemComponents(EntityManager dstManager, Entity entity,
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

                dstManager.AddComponentData(entity, stateMachine);
                dstManager.AddComponentData(entity, AnimationStateMachineTransitionRequest.Null);

                dstManager.AddBuffer<SingleClipState>(entity);
                dstManager.AddBuffer<LinearBlendStateMachineState>(entity);
            }

            //Parameters
            {
                dstManager.AddBuffer<BoolParameter>(entity);
                dstManager.AddBuffer<IntParameter>(entity);
                dstManager.AddBuffer<BlendParameter>(entity);
                foreach (var p in stateMachineAsset.Parameters)
                {
                    switch (p)
                    {
                        case BoolParameterAsset:
                            var boolParameters = dstManager.GetBuffer<BoolParameter>(entity);
                            boolParameters.Add(new BoolParameter(p.name, p.Hash));
                            break;
                        case IntParameterAsset:
                            var intParameters = dstManager.GetBuffer<IntParameter>(entity);
                            intParameters.Add(new IntParameter(p.name, p.Hash));
                            break;
                        case FloatParameterAsset:
                            var floatParameters = dstManager.GetBuffer<BlendParameter>(entity);
                            floatParameters.Add(new BlendParameter(p.name, p.Hash));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(p));
                    }
                }
            }

#if UNITY_EDITOR || DEBUG
            dstManager.AddComponentData(entity, new AnimationStateMachineDebug
            {
                StateMachineAsset = stateMachineAsset
            });
#endif
        }

        public static void AddAnimatorOwnerComponents(EntityManager dstManager, Entity ownerEntity, Entity entity)
        {
            dstManager.AddComponentData(ownerEntity, new AnimatorOwner { AnimatorEntity = entity });
            dstManager.AddComponentData(entity, new AnimatorEntity { Owner = ownerEntity });
        }

        public static void AddRootMotionComponents(EntityManager dstManager, Entity ownerEntity, Entity entity,
            RootMotionMode rootMotionMode)
        {
            switch (rootMotionMode)
            {
                case RootMotionMode.Disabled:
                    break;
                case RootMotionMode.EnabledAutomatic:
                    dstManager.AddComponentData(entity, new RootDeltaTranslation());
                    dstManager.AddComponentData(entity, new RootDeltaRotation());
                    if (ownerEntity != entity)
                    {
                        dstManager.AddComponentData(ownerEntity, new TransferRootMotionToOwner());
                    }
                    else
                    {
                        dstManager.AddComponentData(entity, new ApplyRootMotionToEntity());
                    }

                    break;
                case RootMotionMode.EnabledManual:
                    dstManager.AddComponentData(entity, new RootDeltaTranslation());
                    dstManager.AddComponentData(entity, new RootDeltaRotation());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}