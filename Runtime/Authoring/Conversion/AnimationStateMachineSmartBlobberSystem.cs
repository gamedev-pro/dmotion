using System.Collections.Generic;
using System.Linq;
using Latios.Authoring;
using Latios.Authoring.Systems;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Latios.Kinemation.Authoring.Systems;
using Unity.Assertions;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public struct StateMachineBlobBakeData
    {
        internal AnimationStateMachineAsset StateMachineAsset;
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
            //build states
            {
                converter.SingleClipStates = new UnsafeList<SingleClipStateBlob>(stateMachineAsset.SingleClipStates.Count, allocator);
                converter.SingleClipStates.Resize(stateMachineAsset.SingleClipStates.Count);
                for (ushort i = 0; i < converter.SingleClipStates.Length; i++)
                {
                    converter.SingleClipStates[i] = new SingleClipStateBlob()
                    {
                        ClipIndex = i,
                        Loop = false,
                        Speed = 1
                    };
                }

                var allStatesCount = converter.SingleClipStates.Length;
                converter.States =
                    new UnsafeList<AnimationStateBlob>(allStatesCount, allocator);
                converter.States.Resize(allStatesCount);

                for (short i = 0; i < converter.SingleClipStates.Length; i++)
                {
                    converter.States[i] = new AnimationStateBlob
                    {
                        Type = StateType.Single,
                        StateIndex = i,
                    };
                }
            }
            
            //build parameters
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

            //build transitions
            {
                converter.Transitions = new UnsafeList<TransitionGroupConvertData>(stateMachineAsset.Transitions.Count, allocator);
                converter.Transitions.Resize(stateMachineAsset.Transitions.Count);
                for (short i = 0; i < converter.Transitions.Length; i++)
                {
                    var transitionAsset = stateMachineAsset.Transitions[i];
                    var transitionGroup = new TransitionGroupConvertData();
                    
                    transitionGroup.NormalizedTransitionDuration = transitionAsset.NormalizedTransitionDuration;
                    
                    transitionGroup.FromStateIndex = (short) stateMachineAsset.SingleClipStates.FindIndex(s => s == transitionAsset.FromState);
                    Assert.IsTrue(transitionGroup.FromStateIndex >= 0, $"State {transitionAsset.FromState.name} not present on State Machine {stateMachineAsset.name}");
                    transitionGroup.ToStateIndex = (short) stateMachineAsset.SingleClipStates.FindIndex(s => s == transitionAsset.ToState);
                    Assert.IsTrue(transitionGroup.ToStateIndex >= 0, $"State {transitionAsset.ToState.name} not present on State Machine {stateMachineAsset.name}");

                    transitionGroup.BoolTransitions =
                        new UnsafeList<BoolTransition>(transitionAsset.BoolTransitions.Count, allocator);
                    for (short j = 0; j < transitionGroup.BoolTransitions.Length; j++)
                    {
                        var boolTransitionAsset = transitionAsset.BoolTransitions[i];
                        var parameterIndex = -1;
                        var parameterHash = boolTransitionAsset.Parameter.Hash;
                        for (short k = 0; k < converter.Parameters.Length; k++)
                        {
                            if (converter.Parameters[k].Hash == parameterHash)
                            {
                                parameterIndex = k;
                                break;
                            }
                        }
                        
                        Assert.IsTrue(parameterIndex >= 0, $"({stateMachineAsset.name}) Couldn't find parameter {boolTransitionAsset.Parameter.Name}, for transition");
                        transitionGroup.BoolTransitions[i] = new BoolTransition()
                        {
                            ParameterIndex = parameterIndex,
                            GroupIndex = i,
                            ComparisonValue = boolTransitionAsset.ComparisonValue,
                        };
                    }
                }
            }
            
            //build events (TODO)
            {
                converter.Events = new UnsafeList<AnimationEventBlob>(0, allocator);
                converter.Events.Resize(0);
            }
            return true;
        }
    }
}