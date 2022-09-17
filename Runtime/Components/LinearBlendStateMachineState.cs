using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal struct LinearBlendStateMachineState : IBufferElementData
    {
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal short StateIndex;
        internal byte PlayableId;
        internal readonly ref AnimationStateBlob StateBlob => ref StateMachineBlob.Value.States[StateIndex];
        internal readonly ref LinearBlendStateBlob AsLinearBlend =>
            ref StateMachineBlob.Value.LinearBlendStates[StateBlob.StateIndex];
    }
}