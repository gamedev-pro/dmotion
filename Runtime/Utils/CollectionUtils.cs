using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion
{
    public static class CollectionUtils
    {
        public static NativeArray<T> AsArray<T>(UnsafeList<T> list) where T : unmanaged
        {
            unsafe
            {
                var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(list.Ptr,
                    list.Length, Allocator.None);

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array,
                    AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
                return array;
            }
        }
        
        public static NativeArray<T> AsArray<T>(ref BlobArray<T> arr) where T : struct
        {
            unsafe
            {
                var array = NativeArrayUnsafeUtility.ConvertExistingDataToNativeArray<T>(arr.GetUnsafePtr(),
                    arr.Length, Allocator.None);
#if ENABLE_UNITY_COLLECTIONS_CHECKS
                NativeArrayUnsafeUtility.SetAtomicSafetyHandle(ref array,
                    AtomicSafetyHandle.GetTempUnsafePtrSliceHandle());
#endif
                return array;
            }
        }
    }
}