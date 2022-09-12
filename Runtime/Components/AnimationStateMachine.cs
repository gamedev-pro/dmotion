using System;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal struct StateMachineStateRef
    {
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal ushort StateIndex;
        internal sbyte PlayableId;
        internal bool IsValid => PlayableId >= 0;

        internal static StateMachineStateRef Null => new StateMachineStateRef { PlayableId = -1 };

        internal readonly int IdToIndex(in DynamicBuffer<PlayableState> playableStates)
        {
            return playableStates.IdToIndex((byte)PlayableId);
        }

        internal readonly StateType Type => StateBlob.Type;
        internal readonly ref AnimationStateBlob StateBlob => ref StateMachineBlob.Value.States[StateIndex];
        internal readonly ref SingleClipStateBlob AsSingleClipBlob => ref StateMachineBlob.Value.SingleClipStates[StateBlob.StateIndex];
        internal readonly ref LinearBlendStateBlob AsLinearBlendBlob => ref StateMachineBlob.Value.LinearBlendStates[StateBlob.StateIndex];
    }
    
    [BurstCompile]
    internal struct AnimationStateMachine : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> ClipsBlob;
        internal BlobAssetReference<ClipEventsBlob> ClipEventsBlob;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal StateMachineStateRef CurrentState;
        internal StateMachineStateRef NextState;
        internal float Weight;
        internal float CurrentTransitionDuration;
    }
}