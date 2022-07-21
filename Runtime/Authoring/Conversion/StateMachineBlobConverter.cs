using Latios.Authoring.Systems;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DOTSAnimation.Authoring
{
    internal struct LinearBlendStateConversionData
    {
        internal UnsafeList<DOTSAnimation.ClipWithThreshold> ClipsWithThresholds;
        internal ushort BlendParameterIndex;
    }
    internal struct StateMachineBlobConverter : ISmartBlobberSimpleBuilder<StateMachineBlob>
    {
        internal byte DefaultStateIndex;
        internal UnsafeList<SingleClipStateBlob> SingleClipStates;
        internal UnsafeList<LinearBlendStateConversionData> LinearBlendStates;
        internal UnsafeList<AnimationStateBlob> States;
        internal UnsafeList<DOTSAnimation.AnimationTransitionGroup> Transitions;
        internal UnsafeList<BoolTransition> BoolTransitions;

        public unsafe BlobAssetReference<StateMachineBlob> BuildBlob()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<StateMachineBlob>();
            root.DefaultStateIndex = DefaultStateIndex;
            builder.ConstructFromNativeArray(ref root.SingleClipStates, SingleClipStates.Ptr, SingleClipStates.Length);
            builder.ConstructFromNativeArray(ref root.States, States.Ptr, States.Length);
            builder.ConstructFromNativeArray(ref root.Transitions, Transitions.Ptr, Transitions.Length);
            builder.ConstructFromNativeArray(ref root.BoolTransitions, BoolTransitions.Ptr, BoolTransitions.Length);

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
            
            return builder.CreateBlobAssetReference<StateMachineBlob>(Allocator.Persistent);
        }
    }
}