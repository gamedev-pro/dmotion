using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    public static class ECSTestUtils
    {
        public static void AssertSystemQueries<T>(World world) where T : SystemBase
        {
            var system = world.GetExistingSystem<T>();
            Assert.NotNull(system, $"Couldn't find system of type {typeof(T).Name}");
            Assert.NotZero(system.EntityQueries.Length, "System has no queries to test");
            for (var i = 0; i < system.EntityQueries.Length; i++)
            {
                var q = system.EntityQueries[i];
                Assert.NotZero(q.CalculateEntityCount(), $"Query of index {i} has no matches");
            }
        }
    }
}