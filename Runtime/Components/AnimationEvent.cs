using System;
using Unity.Entities;

namespace DOTSAnimation
{
    public struct RaisedAnimationEvent : IBufferElementData
    {
        public int EventHash;
    }
    
    internal struct AnimationClipEvent
    {
        internal short ClipIndex;
        internal int EventHash;
        internal float NormalizedTime;
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