using System;
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
    internal struct AnimationStateMachine : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> ClipsBlob;
        internal BlobAssetReference<ClipEventsBlob> ClipEventsBlob;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal StateMachineStateRef CurrentState;
        
        //for now we don't use PreviousState for anything other than debugging
        #if UNITY_EDITOR || DEBUG
        internal StateMachineStateRef PreviousState;
        #endif

        internal ref AnimationStateBlob CurrentStateBlob
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ref StateMachineBlob.Value.States[CurrentState.StateIndex];
        }
    }
    
    #if UNITY_EDITOR || DEBUG
    internal class AnimationStateMachineDebug : IComponentData, ICloneable
    {
        internal StateMachineAsset StateMachineAsset;
        public object Clone()
        {
            return new AnimationStateMachineDebug
            {
                StateMachineAsset = StateMachineAsset
            };
        }
    }
    #endif
}