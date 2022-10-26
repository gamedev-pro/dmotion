using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    public static class AnimationEventUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasEventRaised(this DynamicBuffer<RaisedAnimationEvent> raisedEvents, int eventHash, out int index)
        {
            for (var i = 0; i < raisedEvents.Length; i++)
            {
                if (raisedEvents[i].EventHash == eventHash)
                {
                    index = 0;
                    return true;
                }
            }

            index = -1;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasEventRaised(this DynamicBuffer<RaisedAnimationEvent> raisedEvents, int eventHash)
        {
            return raisedEvents.WasEventRaised(eventHash, out _);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasEventRaised(this DynamicBuffer<RaisedAnimationEvent> raisedEvents, FixedString64Bytes eventName,
            out int index)
        {
            return raisedEvents.WasEventRaised(eventName.GetHashCode(), out index);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool WasEventRaised(this DynamicBuffer<RaisedAnimationEvent> raisedEvents, FixedString64Bytes eventName)
        {
            return raisedEvents.WasEventRaised(eventName, out _);
        }
    }
}