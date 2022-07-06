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
    
    public struct AnimationEvent : IBufferElementData, IComparable<AnimationEvent>
    {
        internal int EventHash;
        internal int SamplerIndex;
        internal float NormalizedTime;
        public int CompareTo(AnimationEvent other)
        {
            return NormalizedTime.CompareTo(other.NormalizedTime);
        }
    }
}