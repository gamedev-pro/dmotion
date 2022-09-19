using Unity.Profiling;

namespace DMotion
{
    internal static class ProfilingUtils
    {
        internal static ProfilerMarker CreateMarker<T>(ProfilerCategory category, string operationName)
        {
            return new ProfilerMarker(category, $"{typeof(T).Name}.{operationName}");
        }

        internal static ProfilerMarker CreateAnimationMarker<T>(string operationName)
        {
            return CreateMarker<T>(ProfilerCategory.Animation, operationName);
        }
    }
}