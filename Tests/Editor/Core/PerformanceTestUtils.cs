using System.Reflection;
using NUnit.Framework;
using Unity.Profiling;

namespace DMotion.Tests
{
    public static class PerformanceTestUtils
    {
        public static string GetNameSlow(this ProfilerMarker marker)
        {
            var method = marker.GetType().GetMethod("GetName", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "Couldn't find GetName method");
            var args = new object[1];
            method.Invoke(marker, args);
            var name = (string)args[0];
            Assert.NotNull(name);
            Assert.IsNotEmpty(name);
            return name;
        }
    }
}