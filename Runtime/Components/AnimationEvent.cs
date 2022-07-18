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
    
    internal struct AnimationEventBlob
    {
        internal int EventHash;
        internal int StateIndex;
        internal float NormalizedTime;
    }
}