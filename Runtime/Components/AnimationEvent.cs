using System;
using Latios.Kinemation;
using Unity.Entities;

namespace DMotion
{
    public struct RaisedAnimationEvent : IBufferElementData
    {
        public int EventHash;
        public float ClipWeight;
        public SkeletonClipHandle ClipHandle;
    }
    
    internal struct AnimationClipEvent
    {
        internal short ClipIndex;
        internal int EventHash;
        internal float ClipTime;
    }

    internal struct ClipEvents
    {
        internal BlobArray<AnimationClipEvent> Events;
    }
    
    public struct ClipEventsBlob
    {
        internal BlobArray<ClipEvents> ClipEvents;
    }
}