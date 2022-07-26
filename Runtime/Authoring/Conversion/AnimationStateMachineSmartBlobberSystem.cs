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
            BuildTransitionGroups(stateMachineAsset, ref converter, allocator);
            BuildBoolTransitions(stateMachineAsset, ref converter, allocator);

            return true;
        }

        private void BuildStates(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter,
            Allocator allocator)
        {
            converter.States =
                new UnsafeList<AnimationStateBlob>(stateMachineAsset.StateCount, allocator);
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

                    converter.States[stateIndex] = new AnimationStateBlob
                    {
                        Type = StateType.Single,
                        StateIndex = i,
                        Loop = singleStateAsset.Loop,
                        Speed = singleStateAsset.Speed
                    };
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

                    converter.States[stateIndex] = new AnimationStateBlob
                    {
                        Type = StateType.LinearBlend,
                        StateIndex = i,
                        Loop = linearBlendStateAsset.Loop,
                        Speed = linearBlendStateAsset.Speed
                    };
                    stateIndex++;
                }
            }
        }

        private void BuildTransitionGroups(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter,
            Allocator allocator)
        {
            converter.Transitions =
                new UnsafeList<DMotion.AnimationTransitionGroup>(stateMachineAsset.Transitions.Count, allocator);
            converter.Transitions.Resize(stateMachineAsset.Transitions.Count);
            for (short i = 0; i < converter.Transitions.Length; i++)
            {
                var transitionGroup = new DMotion.AnimationTransitionGroup();
                var transitionAsset = stateMachineAsset.Transitions[i];
                transitionGroup.NormalizedTransitionDuration = transitionAsset.NormalizedTransitionDuration;

                transitionGroup.FromStateIndex =
                    (short)stateMachineAsset.States.ToList().FindIndex(s => s == transitionAsset.FromState);
                Assert.IsTrue(transitionGroup.FromStateIndex >= 0,
                    $"State {transitionAsset.FromState.name} not present on State Machine {stateMachineAsset.name}");
                transitionGroup.ToStateIndex =
                    (short)stateMachineAsset.States.ToList().FindIndex(s => s == transitionAsset.ToState);
                Assert.IsTrue(transitionGroup.ToStateIndex >= 0,
                    $"State {transitionAsset.ToState.name} not present on State Machine {stateMachineAsset.name}");

                converter.Transitions[i] = transitionGroup;
            }
        }

        private void BuildBoolTransitions(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter,
            Allocator allocator)
        {
            var boolTransitionCount = stateMachineAsset.Transitions.Sum(t => t.BoolTransitions.Count);
            converter.BoolTransitions = new UnsafeList<BoolTransition>(boolTransitionCount, allocator);
            converter.BoolTransitions.Resize(boolTransitionCount);
            short boolTransitionIndex = 0;
            for (short groupIndex = 0; groupIndex < stateMachineAsset.Transitions.Count; groupIndex++)
            {
                var transitionGroup = stateMachineAsset.Transitions[groupIndex];
                for (short i = 0; i < transitionGroup.BoolTransitions.Count; i++)
                {
                    var boolTransition = transitionGroup.BoolTransitions[i];
                    var parameterIndex = stateMachineAsset.BoolParameters.FindIndex(p => p == boolTransition.Parameter);
                    Assert.IsTrue(parameterIndex >= 0,
                        $"({stateMachineAsset.name}) Couldn't find parameter {boolTransition.Parameter.Name}, for transition");
                    converter.BoolTransitions[boolTransitionIndex] = new BoolTransition()
                    {
                        GroupIndex = groupIndex,
                        ComparisonValue = boolTransition.ComparisonValue,
                        ParameterIndex = parameterIndex
                    };
                    boolTransitionIndex++;
                }
            }
        }
    }
}