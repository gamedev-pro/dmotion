using System.Runtime.CompilerServices;
using Unity.Assertions;
using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal static class ClipSamplerUtils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte AddWithId(this DynamicBuffer<ClipSampler> samplers, ClipSampler newSampler)
        {
            if (samplers.TryFindIdAndInsertIndex(1, out var id, out var insertIndex))
            {
                newSampler.Id = id;
                samplers.Insert(insertIndex, newSampler);
            }
            return newSampler.Id;
        }

        internal static bool TryFindIdAndInsertIndex(this DynamicBuffer<ClipSampler> samplers, byte reserveCount, out byte id, out int insertIndex)
        {
            //we assume the list is always sorted (should be true if Id always increments  from 0 to 128 and loops back)
            //on the loop back case, we add after the first element for which we can ensure reserveCount
            const byte maxValue = byte.MaxValue / 2;
            
            //sanity check
            const byte maxReserveCount = 20;
            Assert.IsTrue(reserveCount <= maxReserveCount, "Reserve count too high. Why are you trying to allocate so many contiguous clips?");

            if (samplers.Length == 0)
            {
                id = 0;
                insertIndex = 0;
                return true;
            }

            var last = samplers[^1];
            int idWithReserveCount = last.Id + reserveCount;
            if (idWithReserveCount <= maxValue)
            {
                //impossible to overflow
                id = (byte) (last.Id + 1);
                insertIndex = samplers.Length;
                return true;
            }
            
            //possible loopback case
            
            //our last samplers had the max id, but there is no one else in the list. Give id 0
            if (samplers.Length == 1)
            {
                id = 0;
                insertIndex = 0;
                return true;
            }
            
            //find first sampler for which we can have reserveCount contiguous indexes
            for (var i = 0; i < samplers.Length - 1; i++)
            {
                var current = samplers[i];
                var next = samplers[i + 1];

                if (next.Id - current.Id < reserveCount)
                {
                    id = (byte) (current.Id + 1);
                    insertIndex = i + 1;
                    return true;
                }
            }
            
            // From my current understand, we can only be here if if (it's possible I'm wrong though): 
            // 1 - reserveCount is massive (asserted above), or 2 - we managed to get a very fragmented id space
            // We don't handle this case (it's not reasonable), so let's scream
            Assert.IsTrue(false, "This is a bug. I don't know how we could ever be here");
            id = 0;
            insertIndex = -1;
            return false;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool RemoveWithId(this DynamicBuffer<ClipSampler> samplers, byte id)
        {
            return RemoveRangeWithId(samplers, id, 1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool RemoveRangeWithId(this DynamicBuffer<ClipSampler> samplers, byte id, byte count)
        {
            var index = samplers.IdToIndex(id);
            var exists = index >= 0;
            if (exists)
            {
                samplers.RemoveRange(index, count);
            }

            return exists;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static int IdToIndex(this DynamicBuffer<ClipSampler> samplers, byte id)
        {
            for (var i = 0; i < samplers.Length; i++)
            {
                if (samplers[i].Id == id)
                {
                    return i;
                }
            }
            return -1;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetById(this DynamicBuffer<ClipSampler> samplers, byte id, out ClipSampler sampler)
        {
            var index = samplers.IdToIndex(id);
            if (index >= 0)
            {
                sampler = samplers[index];
                return true;
            }
            sampler = default;
            return false;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TrySet(this DynamicBuffer<ClipSampler> samplers, in ClipSampler sampler)
        {
            var index = samplers.IdToIndex(sampler.Id);
            if (index >= 0)
            {
                samplers[index] = sampler;
                return true;
            }
            return false;
        }
    }
}