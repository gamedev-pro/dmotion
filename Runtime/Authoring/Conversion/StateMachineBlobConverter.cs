using Latios.Authoring.Systems;
using Latios.Kinemation;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DOTSAnimation.Authoring
{
    internal struct TransitionGroupConvertData
    {
        internal short FromStateIndex;
        internal short ToStateIndex;
        internal float NormalizedTransitionDuration;
        internal UnsafeList<BoolTransition> BoolTransitions;
    }
    internal struct StateMachineBlobConverter : ISmartBlobberSimpleBuilder<StateMachineBlob>
    {
        internal UnsafeList<SingleClipStateBlob> SingleClipStates;
        internal UnsafeList<AnimationStateBlob> States;
        internal UnsafeList<StateMachineParameter> Parameters;
        internal UnsafeList<TransitionGroupConvertData> Transitions;
        internal UnsafeList<AnimationEventBlob> Events;

        public unsafe BlobAssetReference<StateMachineBlob> BuildBlob()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<StateMachineBlob>();
            builder.ConstructFromNativeArray(ref root.SingleClipStates, SingleClipStates.Ptr, SingleClipStates.Length);
            builder.ConstructFromNativeArray(ref root.States, States.Ptr, States.Length);
            builder.ConstructFromNativeArray(ref root.Parameters, Parameters.Ptr, Parameters.Length);
            
            var transitionGroups = builder.Allocate(ref root.Transitions, Transitions.Length);
            for (short i = 0; i < transitionGroups.Length; i++)
            {
                var transitionGroupConvertData = Transitions[i];
                builder.ConstructFromNativeArray(ref transitionGroups[i].BoolTransitions,
                    transitionGroupConvertData.BoolTransitions.Ptr, transitionGroupConvertData.BoolTransitions.Length);
            }
            
            builder.ConstructFromNativeArray(ref root.Events, Events.Ptr, Events.Length);
            return builder.CreateBlobAssetReference<StateMachineBlob>(Allocator.Persistent);
        }
    }
}