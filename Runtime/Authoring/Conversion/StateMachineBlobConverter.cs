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
        internal UnsafeList<DOTSAnimation.AnimationClipEvent> ClipEvents;
        internal UnsafeList<DOTSAnimation.AnimationTransitionGroup> Transitions;
        internal UnsafeList<BoolTransition> BoolTransitions;

        public unsafe BlobAssetReference<StateMachineBlob> BuildBlob()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<StateMachineBlob>();
            builder.ConstructFromNativeArray(ref root.SingleClipStates, SingleClipStates.Ptr, SingleClipStates.Length);
            builder.ConstructFromNativeArray(ref root.States, States.Ptr, States.Length);
            builder.ConstructFromNativeArray(ref root.Parameters, Parameters.Ptr, Parameters.Length);
            builder.ConstructFromNativeArray(ref root.ClipEvents, ClipEvents.Ptr, ClipEvents.Length);
            builder.ConstructFromNativeArray(ref root.Transitions, Transitions.Ptr, Transitions.Length);
            builder.ConstructFromNativeArray(ref root.BoolTransitions, BoolTransitions.Ptr, BoolTransitions.Length);
            
            return builder.CreateBlobAssetReference<StateMachineBlob>(Allocator.Persistent);
        }
    }
}