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
    
    internal struct AnimationClipEvent
    {
        internal short ClipIndex;
        internal int EventHash;
        internal float NormalizedTime;
    }
}