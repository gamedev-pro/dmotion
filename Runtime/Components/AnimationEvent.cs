using System;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate void AnimationEventDelegate(Entity* owner, int sortKey, EntityCommandBuffer.ParallelWriter* ecb);
    public struct AnimationEvent : IBufferElementData, IComparable<AnimationEvent>
    {
        internal int SamplerIndex;
        internal float NormalizedTime;
        internal FunctionPointer<AnimationEventDelegate> Delegate;
        public int CompareTo(AnimationEvent other)
        {
            return NormalizedTime.CompareTo(other.NormalizedTime);
        }
    }
}