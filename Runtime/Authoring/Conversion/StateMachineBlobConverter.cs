using System.Collections.Generic;
using Latios.Authoring.Systems;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion.Authoring
{
    internal struct AnimationStateConversionData
    {
        internal StateType Type;
        internal ushort StateIndex;
        internal bool Loop;
        internal float Speed;
        internal UnsafeList<StateOutTransitionConversionData> Transitions;
    }

    internal struct ClipIndexWithThreshold
    {
        internal int ClipIndex;
        internal float Threshold;
    }
    
    internal struct LinearBlendStateConversionData
    {
        internal UnsafeList<ClipIndexWithThreshold> ClipsWithThresholds;
        internal ushort BlendParameterIndex;
    }
    
    internal struct StateOutTransitionConversionData
    {
        internal short ToStateIndex;
        internal float TransitionDuration;
        internal float TransitionEndTime;
        internal UnsafeList<BoolTransition> BoolTransitions;
    }
    
    internal struct StateMachineBlobConverter : ISmartBlobberSimpleBuilder<StateMachineBlob>, IComparer<ClipIndexWithThreshold>
    {
        internal byte DefaultStateIndex;
        internal UnsafeList<AnimationStateConversionData> States;
        internal UnsafeList<SingleClipStateBlob> SingleClipStates;
        internal UnsafeList<LinearBlendStateConversionData> LinearBlendStates;

        public unsafe BlobAssetReference<StateMachineBlob> BuildBlob()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<StateMachineBlob>();
            root.DefaultStateIndex = DefaultStateIndex;
            builder.ConstructFromNativeArray(ref root.SingleClipStates, SingleClipStates.Ptr, SingleClipStates.Length);

            //States
            {
                var states = builder.Allocate(ref root.States, States.Length);
                for (ushort stateIndex = 0; stateIndex < states.Length; stateIndex++)
                {
                    var stateConversionData = States[stateIndex];
                    states[stateIndex] = new AnimationStateBlob()
                    {
                        Type = stateConversionData.Type,
                        StateIndex = stateConversionData.StateIndex,
                        Loop = stateConversionData.Loop,
                        Speed = stateConversionData.Speed,
                    };
                    
                    //transitions
                    var transitions = builder.Allocate(ref states[stateIndex].Transitions, stateConversionData.Transitions.Length);
                    for (ushort transitionIndex = 0; transitionIndex < transitions.Length; transitionIndex++)
                    {
                        var transitionConversionData = stateConversionData.Transitions[transitionIndex];
                        transitions[transitionIndex] = new StateOutTransitionGroup()
                        {
                            ToStateIndex = transitionConversionData.ToStateIndex,
                            TransitionEndTime = transitionConversionData.TransitionEndTime,
                            TransitionDuration = transitionConversionData.TransitionDuration
                        };
                        
                        builder.ConstructFromNativeArray(
                            ref transitions[transitionIndex].BoolTransitions,
                            transitionConversionData.BoolTransitions.Ptr,
                            transitionConversionData.BoolTransitions.Length);
                    }
                }
            }
            
            //Linear Blend state
            {
                var linearBlendStates = builder.Allocate(ref root.LinearBlendStates, LinearBlendStates.Length);
                for (ushort i = 0; i < linearBlendStates.Length; i++)
                {
                    var linearBlendStateConversionData = LinearBlendStates[i];
                    linearBlendStates[i] = new LinearBlendStateBlob
                        { BlendParameterIndex = linearBlendStateConversionData.BlendParameterIndex };

                    //TODO: Actually sort things first
                    //Make sure clips are sorted by threshold
                    var clipsArray = CollectionUtils.AsArray(linearBlendStateConversionData.ClipsWithThresholds);
                    clipsArray.Sort(this);
                    
                    var sortedIndexes = builder.Allocate(ref linearBlendStates[i].SortedClipIndexes, clipsArray.Length);
                    var sortedThresholds = builder.Allocate(ref linearBlendStates[i].SortedClipThresholds, clipsArray.Length);

                    for (var clipIndex = 0; clipIndex < clipsArray.Length; clipIndex++)
                    {
                        var clip = clipsArray[clipIndex];
                        sortedIndexes[clipIndex] = clip.ClipIndex;
                        sortedThresholds[clipIndex] = clip.Threshold;
                    }
                }
            }
            
            return builder.CreateBlobAssetReference<StateMachineBlob>(Allocator.Persistent);
        }

        public int Compare(ClipIndexWithThreshold x, ClipIndexWithThreshold y)
        {
            return x.Threshold.CompareTo(y.Threshold);
        }
    }
}