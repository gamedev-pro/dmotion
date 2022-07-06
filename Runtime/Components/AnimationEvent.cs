using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
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