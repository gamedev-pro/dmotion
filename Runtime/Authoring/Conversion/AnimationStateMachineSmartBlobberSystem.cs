using System.Collections.Generic;
using System.Linq;
using Latios.Authoring;
using Latios.Authoring.Systems;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Latios.Kinemation.Authoring.Systems;
using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public struct StateMachineBlobBakeData
    {
        internal StateMachineAsset StateMachineAsset;
    }

    public static class AnimationStateMachineConversionUtils
    {
        public static SmartBlobberHandle<StateMachineBlob> CreateBlob(
            this GameObjectConversionSystem conversionSystem,
            GameObject gameObject,
            StateMachineBlobBakeData bakeData)
        {
            return conversionSystem.World.GetExistingSystem<AnimationStateMachineSmartBlobberSystem>()
                .AddToConvert(gameObject, bakeData);
        }
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
            BuildStates(stateMachineAsset, ref converter, allocator);
            BuildParameters(stateMachineAsset, ref converter, allocator);
            BuildTransitionGroups(stateMachineAsset, ref converter, allocator);
            BuildBoolTransitions(stateMachineAsset, ref converter, allocator);
            BuildEvents(stateMachineAsset, ref converter, allocator);
            
            return true;
        }

        private void BuildStates(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter, Allocator allocator)
        {
            converter.SingleClipStates = new UnsafeList<SingleClipStateBlob>(stateMachineAsset.SingleClipStates.Count, allocator);
            converter.SingleClipStates.Resize(stateMachineAsset.SingleClipStates.Count);
            for (ushort i = 0; i < converter.SingleClipStates.Length; i++)
            {
                var singleStateAsset = stateMachineAsset.SingleClipStates[i];
                converter.SingleClipStates[i] = new SingleClipStateBlob()
                {
                    ClipIndex = i,
                };
            }

            converter.States =
                new UnsafeList<AnimationStateBlob>(stateMachineAsset.StateCount, allocator);
            converter.States.Resize(stateMachineAsset.StateCount);

            short stateIndex = 0;
            foreach (var state in stateMachineAsset.States)
            {
                converter.States[stateIndex] = new AnimationStateBlob
                {
                    Type = StateType.Single,
                    StateIndex = stateIndex,
                    Loop = state.Loop,
                    Speed = state.Speed
                };
                stateIndex++;
            }
        }

        private void BuildParameters(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter,
            Allocator allocator)
        {
            converter.Parameters = new UnsafeList<StateMachineParameter>(stateMachineAsset.Parameters.Count, allocator);
            converter.Parameters.Resize(stateMachineAsset.Parameters.Count);
            for (short i = 0; i < converter.Parameters.Length; i++)
            {
                converter.Parameters[i] = new StateMachineParameter()
                {
                    Hash = stateMachineAsset.Parameters[i].Hash
                };
            }
        }
        private void BuildTransitionGroups(StateMachineAsset stateMachineAsset, ref StateMachineBlobConverter converter,
            Allocator allocator)
        {
            TransitionGroupConvertData BuildTransition(int groupIndex, AnimationTransitionGroup transitionAsset, List<AnimationParameterAsset> parameters)
            {
                var transitionGroup = new TransitionGroupConvertData();
                transitionGroup.BoolTransitions =
                    new UnsafeList<BoolTransition>(transitionAsset.BoolTransitions.Count, allocator);
                transitionGroup.BoolTransitions.Resize(transitionAsset.BoolTransitions.Count);
                for (short i = 0; i < transitionGroup.BoolTransitions.Length; i++)
                {
                    var boolTransitionAsset = transitionAsset.BoolTransitions[i];
                    var parameterIndex = parameters.FindIndex(p => p == boolTransitionAsset.Parameter);
                    Assert.IsTrue(parameterIndex >= 0, $"({stateMachineAsset.name}) Couldn't find parameter {boolTransitionAsset.Parameter.Name}, for transition");
                    transitionGroup.BoolTransitions[i] = new BoolTransition()
                    {
                        ParameterIndex = parameterIndex,
                        GroupIndex = groupIndex,
                        ComparisonValue = boolTransitionAsset.ComparisonValue,
                    };
                }

                return transitionGroup;
            }

            converter.Transitions = new UnsafeList<DOTSAnimation.AnimationTransitionGroup>(stateMachineAsset.Transitions.Count, allocator);
            converter.Transitions.Resize(stateMachineAsset.Transitions.Count);
            for (short i = 0; i < converter.Transitions.Length; i++)
            {
                var transitionGroup = new DOTSAnimation.AnimationTransitionGroup();
                var transitionAsset = stateMachineAsset.Transitions[i];
                transitionGroup.NormalizedTransitionDuration = transitionAsset.NormalizedTransitionDuration;
                
                transitionGroup.FromStateIndex = (short) stateMachineAsset.SingleClipStates.FindIndex(s => s == transitionAsset.FromState);
                Assert.IsTrue(transitionGroup.FromStateIndex >= 0, $"State {transitionAsset.FromState.name} not present on State Machine {stateMachineAsset.name}");
                transitionGroup.ToStateIndex = (short) stateMachineAsset.SingleClipStates.FindIndex(s => s == transitionAsset.ToState);
                Assert.IsTrue(transitionGroup.ToStateIndex >= 0, $"State {transitionAsset.ToState.name} not present on State Machine {stateMachineAsset.name}");

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
                    var parameterIndex = stateMachineAsset.Parameters.FindIndex(p => p == boolTransition.Parameter);
                    Assert.IsTrue(parameterIndex >= 0, $"({stateMachineAsset.name}) Couldn't find parameter {boolTransition.Parameter.Name}, for transition");
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

        private void BuildEvents(StateMachineAsset stateMachineAsset,
            ref StateMachineBlobConverter converter,
            Allocator allocator)
        {
            var eventCount = stateMachineAsset.Clips.Sum(c => c.Events.Length);
            converter.ClipEvents = new UnsafeList<DOTSAnimation.AnimationClipEvent>(eventCount, allocator);
            converter.ClipEvents.Resize(eventCount);
            short clipIndex = 0;
            short eventIndex = 0;
            foreach (var clip in stateMachineAsset.Clips)
            {
                for (short i = 0; i < clip.Events.Length; i++)
                {
                    converter.ClipEvents[eventIndex] = new DOTSAnimation.AnimationClipEvent()
                    {
                        ClipIndex = clipIndex,
                        EventHash = clip.Events[i].Hash,
                        NormalizedTime = clip.Events[i].NormalizedTime
                    };
                    eventIndex++;
                }
                clipIndex++;
            }
        }
    }
}