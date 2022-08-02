﻿using Latios.Kinemation;
using Unity.Entities;

namespace DMotion
{
    public struct PlayOneShotRequest : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEvents;
        internal short ClipIndex;
        internal float TransitionDuration;
        internal float EndTime;
        internal float Speed;

        public bool IsValid => ClipIndex >= 0 && Clips.IsCreated;

        public static PlayOneShotRequest Null => new PlayOneShotRequest() { ClipIndex = -1 };

        public PlayOneShotRequest(BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents, int clipIndex,
            float transitionDuration = 0.15f,
            float endTime = 0.8f,
            float speed = 1)
        {
            Clips = clips;
            ClipEvents = clipEvents;
            ClipIndex = (short) clipIndex;
            TransitionDuration = transitionDuration;
            EndTime = endTime;
            Speed = speed;
        }
    }
    internal struct OneShotState : IComponentData
    {
        internal short SamplerIndex;
        internal float TransitionDuration;
        internal float EndTime;
        internal float Speed;

        internal bool IsValid => SamplerIndex >= 0;
        internal static OneShotState Null => new OneShotState() { SamplerIndex = -1 };

        internal OneShotState(int samplerIndex, float transitionDuration,
            float endTime, float speed)
        {
            SamplerIndex = (short) samplerIndex;
            TransitionDuration = transitionDuration;
            EndTime = endTime;
            Speed = speed;
        }
    }
}