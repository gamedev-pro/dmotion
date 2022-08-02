using System;
using System.Linq;
using Latios.Authoring.Systems;
using Latios.Kinemation.Authoring.Systems;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public struct StateMachineBlobBakeData
    {
        internal StateMachineAsset StateMachineAsset;
    }

    [UpdateAfter(typeof(SkeletonClipSetSmartBlobberSystem))]
    internal class AnimationStateMachineSmartBlobberSystem : SmartBlobberConversionSystem<StateMachineBlob,
        StateMachineBlobBakeData, StateMachineBlobConverter>
    {
        protected override bool Filter(in StateMachineBlobBakeData input, GameObject gameObject,
            out StateMachineBlobConverter converter)
        {
            converter = new StateMachineBlobConverter();

            var stateMachineAsset = input.StateMachineAsset;

            var allocator = World.UpdateAllocator.ToAllocator;
            var defaultStateIndex = stateMachineAsset.States.ToList().IndexOf(stateMachineAsset.DefaultState);
            Assert.IsTrue(defaultStateIndex >= 0, $"Couldn't find state {stateMachineAsset.DefaultState.name}, in state machine {stateMachineAsset.name}");
            converter.DefaultStateIndex = (byte)defaultStateIndex;
            BuildStates(stateMachineAsset, ref converter, allocator);

            return true;
        }

        private void BuildStates(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter,
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
                            BlendParameterIndex = (ushort) blendParameterIndex
                        };
                        
                        linearBlendState.ClipsWithThresholds = new UnsafeList<DMotion.ClipWithThreshold>(
                            linearBlendStateAsset.BlendClips.Length, allocator);
                        
                        linearBlendState.ClipsWithThresholds.Resize(linearBlendStateAsset.BlendClips.Length);
                        for (ushort blendClipIndex = 0; blendClipIndex < linearBlendState.ClipsWithThresholds.Length; blendClipIndex++)
                        {
                            linearBlendState.ClipsWithThresholds[blendClipIndex] = new DMotion.ClipWithThreshold()
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

        private AnimationStateConversionData BuildStateConversionData(StateMachineAsset stateMachineAsset, AnimationStateAsset state, int stateIndex, Allocator allocator)
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
                    for (var boolTransitionIndex = 0; boolTransitionIndex < outTransition.BoolTransitions.Length; boolTransitionIndex++)
                    {
                        var boolTransitionAsset = outTransitionAsset.Conditions[boolTransitionIndex];
                        var parameterIndex = stateMachineAsset.Parameters
                            .OfType<BoolParameterAsset>()
                            .ToList()
                            .FindIndex(p => p == boolTransitionAsset.Parameter);
                        
                        Assert.IsTrue(parameterIndex >= 0,
                            $"({stateMachineAsset.name}) Couldn't find parameter {boolTransitionAsset.Parameter.name}, for transition");
                        outTransition.BoolTransitions[boolTransitionIndex] = new BoolTransition()
                        {
                            ComparisonValue = boolTransitionAsset.ComparisonValue == 1.0f,
                            ParameterIndex = parameterIndex
                        };
                    }
                }

                stateConversionData.Transitions[transitionIndex] = outTransition;
            }
            return stateConversionData;
        }
    }
}