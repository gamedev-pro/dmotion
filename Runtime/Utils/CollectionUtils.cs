using Unity.Entities;

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
    }
}