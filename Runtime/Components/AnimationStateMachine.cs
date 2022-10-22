using System.Runtime.CompilerServices;
using DMotion.Authoring;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal struct StateMachineStateRef
    {
        internal ushort StateIndex;
        internal sbyte AnimationStateId;
        internal bool IsValid => AnimationStateId >= 0;

        internal static StateMachineStateRef Null => new() { AnimationStateId = -1 };
    }

    [BurstCompile]
    internal struct AnimationStateMachineTransitionRequest : IComponentData
    {
        internal bool IsRequested;
        internal float TransitionDuration;

        internal static AnimationStateMachineTransitionRequest Null
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => new AnimationStateMachineTransitionRequest { IsRequested = false };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static AnimationStateMachineTransitionRequest New(float transitionDuration)
        {
            return new AnimationStateMachineTransitionRequest()
            {
                IsRequested = true,
                TransitionDuration = transitionDuration
            };
        }
    }

    [BurstCompile]
    internal struct AnimationStateMachine : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> ClipsBlob;
        internal BlobAssetReference<ClipEventsBlob> ClipEventsBlob;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal StateMachineStateRef CurrentState;

        internal ref AnimationStateBlob CurrentStateBlob
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref StateMachineBlob.Value.States[CurrentState.StateIndex];
        }
    }
    
    #if UNITY_EDITOR || DEBUG
    internal class AnimationStateMachineDebug : IComponentData
    {
        internal StateMachineAsset StateMachineAsset;
    }
    #endif
}