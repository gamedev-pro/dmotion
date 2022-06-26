using Unity.Collections;
using Unity.Entities;

namespace DOTSAnimation
{
    public static class ConversionUtils
    {
        public static DynamicBuffer<T> AddBufferData<T>(this EntityManager dstManager, Entity e, params T[] data)
            where T : struct, IBufferElementData
        {
            return dstManager.AddBufferData<T>(e, new NativeArray<T>(data, Allocator.Temp));
        }
        
        public static DynamicBuffer<T> AddBufferData<T>(this EntityManager dstManager, Entity e, NativeArray<T> data) where T : struct, IBufferElementData
        {
            dstManager.AddBuffer<T>(e);
            var buffer = dstManager.GetBuffer<T>(e);
            buffer.AddRange(data);
            return buffer;
        }

        public static DynamicBuffer<T> GetOrCreateBuffer<T>(this EntityManager dstManager, Entity e)
            where T : struct, IBufferElementData
        {
            return dstManager.HasComponent<T>(e)
                ? dstManager.GetBuffer<T>(e)
                : dstManager.AddBuffer<T>(e);
        }
    }
}