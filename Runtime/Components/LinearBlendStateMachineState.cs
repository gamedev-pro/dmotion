using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal struct LinearBlendStateMachineState : IBufferElementData
    {
        internal byte PlayableId;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal short StateIndex;
        internal readonly ref AnimationStateBlob StateBlob => ref StateMachineBlob.Value.States[StateIndex];
        internal readonly ref LinearBlendStateBlob LinearBlendBlob =>
            ref StateMachineBlob.Value.LinearBlendStates[StateBlob.StateIndex];
    }
}