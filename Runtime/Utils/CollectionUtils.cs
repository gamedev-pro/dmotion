using Unity.Entities;
using UnityEngine.Assertions;

namespace DOTSAnimation
{
    public static class CollectionUtils
    {
        public static T ElementAtSafe<T>(this DynamicBuffer<T> buffer, int index) where T : struct
        {
            if (index >= 0 && index < buffer.Length)
            {
                return buffer[index];
            }

            return default;
        }

        public static bool IsValidIndex<T>(this DynamicBuffer<T> buffer, int index) where T : struct
        {
            return (index >= 0 && index < buffer.Length);
        }
        public static T ElementAtSafe<T>(this BlobArray<T> blobArray, int index) where T : struct
        {
            var isValidIndex = index >= 0 && index < blobArray.Length;
            Assert.IsTrue(isValidIndex);
            return isValidIndex ? blobArray[index] : default;
        }
    }
}