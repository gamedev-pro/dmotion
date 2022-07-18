using System;
using Unity.Entities;

namespace DOTSAnimation
{
    public struct RaisedAnimationEvent
    {
        public int EventHash;
        public Entity AnimatorEntity;
        public Entity AnimatorOwner;
    }

    internal struct AnimationClipEventsBlob
    {
        internal BlobArray<ClipEventBlob> Events;
    }
    
    internal struct ClipEventBlob
    {
        internal int EventHash;
        internal float NormalizedTime;
    }
}