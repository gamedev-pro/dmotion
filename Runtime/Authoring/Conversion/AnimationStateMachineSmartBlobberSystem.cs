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
            converter.States =
                new UnsafeList<AnimationStateConversionData>(stateMachineAsset.StateCount, allocator);
            converter.States.Resize(stateMachineAsset.StateCount);

            ushort stateIndex = 0;
            ushort clipIndex = 0;

            //Build single states
            {
                converter.SingleClipStates =
                    new UnsafeList<SingleClipStateBlob>(stateMachineAsset.SingleClipStates.Count, allocator);
                converter.SingleClipStates.Resize(stateMachineAsset.SingleClipStates.Count);
                for (ushort i = 0; i < converter.SingleClipStates.Length; i++)
                {
                    var singleStateAsset = stateMachineAsset.SingleClipStates[i];
                    converter.SingleClipStates[i] = new SingleClipStateBlob()
                    {
                        ClipIndex = clipIndex,
                    };
                    clipIndex++;

                    converter.States[stateIndex] = BuildStateConversionData(stateMachineAsset, singleStateAsset, i, allocator);
                    stateIndex++;
                }
            }

            //Build linear blend states
            {
                converter.LinearBlendStates =
                    new UnsafeList<LinearBlendStateConversionData>(stateMachineAsset.LinearBlendStates.Count,
                        allocator);
                converter.LinearBlendStates.Resize(stateMachineAsset.LinearBlendStates.Count);
                for (ushort i = 0; i < converter.LinearBlendStates.Length; i++)
                {
                    var linearBlendStateAsset = stateMachineAsset.LinearBlendStates[i];
                    var blendParameterIndex =
                        stateMachineAsset.FloatParameters.FindIndex(f => f == linearBlendStateAsset.BlendParameter);

                    Assert.IsTrue(blendParameterIndex >= 0,
                        $"({stateMachineAsset.name}) Couldn't find parameter {linearBlendStateAsset.BlendParameter.Name}, for Linear Blend State");
                    
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

                    converter.LinearBlendStates[i] = linearBlendState;

                    converter.States[stateIndex] = BuildStateConversionData(stateMachineAsset, linearBlendStateAsset, i, allocator);
                    stateIndex++;
                }
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
                    NormalizedTransitionDuration = outTransitionAsset.NormalizedTransitionDuration,
                };

                //Create bool transitions
                {
                    var boolTransitionCount = outTransitionAsset.BoolTransitions.Count;
                    outTransition.BoolTransitions =
                        new UnsafeList<BoolTransition>(outTransitionAsset.BoolTransitions.Count, allocator);
                    outTransition.BoolTransitions.Resize(boolTransitionCount);
                    for (var boolTransitionIndex = 0; boolTransitionIndex < outTransition.BoolTransitions.Length; boolTransitionIndex++)
                    {
                        var boolTransitionAsset = outTransitionAsset.BoolTransitions[boolTransitionIndex];
                        var parameterIndex = stateMachineAsset.BoolParameters.FindIndex(p => p == boolTransitionAsset.Parameter);
                        Assert.IsTrue(parameterIndex >= 0,
                            $"({stateMachineAsset.name}) Couldn't find parameter {boolTransitionAsset.Parameter.Name}, for transition");
                        outTransition.BoolTransitions[boolTransitionIndex] = new BoolTransition()
                        {
                            ComparisonValue = boolTransitionAsset.ComparisonValue,
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