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
    
    internal struct LinearBlendStateConversionData
    {
        internal UnsafeList<DMotion.ClipWithThreshold> ClipsWithThresholds;
        internal ushort BlendParameterIndex;
    }
    
    internal struct StateOutTransitionConversionData
    {
        internal short ToStateIndex;
        internal float NormalizedTransitionDuration;
        internal UnsafeList<BoolTransition> BoolTransitions;
    }
    
    internal struct StateMachineBlobConverter : ISmartBlobberSimpleBuilder<StateMachineBlob>
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
                            NormalizedTransitionDuration = transitionConversionData.NormalizedTransitionDuration
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
                    linearBlendStates[i] = new LinearBlendStateBlob()
                        { BlendParameterIndex = linearBlendStateConversionData.BlendParameterIndex };
                    
                    //TODO: sort by threshold
                    builder.ConstructFromNativeArray(
                        ref linearBlendStates[i].ClipSortedByThreshold,
                        linearBlendStateConversionData.ClipsWithThresholds.Ptr,
                        linearBlendStateConversionData.ClipsWithThresholds.Length);
                }
            }
            
            return builder.CreateBlobAssetReference<StateMachineBlob>(Allocator.Persistent);
        }
    }
}