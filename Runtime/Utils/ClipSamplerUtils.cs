using System.Runtime.CompilerServices;
using Unity.Assertions;
using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal static class ClipSamplerUtils
    {
        internal const int MaxReserveCount = 30;
        internal const int MaxSamplersCount = byte.MaxValue / 2;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool AddWithId<T>(this DynamicBuffer<T> samplers, T newSampler, out byte id, out int index)
            where T : unmanaged, IElementWithId
        {
            if (samplers.TryFindIdAndInsertIndex(1, out id, out index))
            {
                newSampler.Id = id;
                samplers.Insert(index, newSampler);
                return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static byte AddWithId<T>(this DynamicBuffer<T> samplers, T newSampler) where T : unmanaged, IElementWithId
        {
            samplers.AddWithId(newSampler, out var id, out _);
            return id;
        }

        internal static bool TryFindIdAndInsertIndex<T>(this DynamicBuffer<T> samplers, byte reserveCount, out byte id,
            out int insertIndex) where T : unmanaged, IElementWithId
        {
            //we assume the list is always sorted (should be true if Id always increments  from 0 to 128 and loops back)
            //on the loop back case, we add after the first element for which we can ensure reserveCount
            Assert.IsTrue(samplers.Length + reserveCount < MaxSamplersCount, "No support for more than 128 clips");

            //sanity check
            Assert.IsTrue(reserveCount <= MaxReserveCount,
                "Reserve count too high. Why are you trying to allocate so many contiguous clips?");

            if (samplers.Length == 0)
            {
                id = 0;
                insertIndex = 0;
                return true;
            }

            var last = samplers[^1];
            int idWithReserveCount = last.Id + reserveCount;
            if (idWithReserveCount < MaxSamplersCount)
            {
                //impossible to overflow
                id = (byte)(last.Id + 1);
                insertIndex = samplers.Length;
                return true;
            }

            //possible loopback case
            //our last samplers had the max id, but there is no one else in the list. Give id 0
            if (samplers.Length == 1)
            {
                id = 0;
                insertIndex = samplers.Length;
                return true;
            }

            //find first sampler for which we can have reserveCount contiguous indexes
            for (var i = 0; i < samplers.Length - 1; i++)
            {
                var current = samplers[i];
                var next = samplers[i + 1];

                if (next.Id - current.Id > reserveCount)
                {
                    id = (byte)(current.Id + 1);
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
        internal static bool RemoveWithId<T>(this DynamicBuffer<T> samplers, byte id) where T : unmanaged, IElementWithId
        {
            return RemoveRangeWithId(samplers, id, 1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool RemoveRangeWithId<T>(this DynamicBuffer<T> samplers, byte id, byte count)
            where T : unmanaged, IElementWithId
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
        internal static int IdToIndex<T>(this DynamicBuffer<T> samplers, byte id) where T : unmanaged, IElementWithId
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
        internal static bool ExistsWithId<T>(this DynamicBuffer<T> samplers, byte id) where T : unmanaged, IElementWithId
        {
            return samplers.IdToIndex(id) >= 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool TryGetWithId<T>(this DynamicBuffer<T> samplers, byte id, out T element)
            where T : unmanaged, IElementWithId
        {
            var index = samplers.IdToIndex(id);
            if (index >= 0)
            {
                element = samplers[index];
                return true;
            }

            element = default;
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static T GetWithId<T>(this DynamicBuffer<T> elements, byte id) where T : unmanaged, IElementWithId
        {
            var success = elements.TryGetWithId(id, out var e);
            Assert.IsTrue(success);
            return e;
        }
    }
}