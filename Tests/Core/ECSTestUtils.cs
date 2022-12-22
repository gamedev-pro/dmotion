using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    public static class ECSTestUtils
    {
        public static void AssertSystemQueriesManaged<T>(World world) where T : SystemBase
        {
            var system = world.GetExistingSystemManaged<T>();
            Assert.NotNull(system, $"Couldn't find system of type {typeof(T).Name}");
            Assert.NotZero(system.EntityQueries.Length, "System has no queries to test");
            for (var i = 0; i < system.EntityQueries.Length; i++)
            {
                var q = system.EntityQueries[i];
                Assert.NotZero(q.CalculateEntityCount(), $"Query of index {i} has no matches");
            }
        }

        public static void AssertSystemQueries<T>(World world) where T : unmanaged, ISystem
        {
            var system = world.GetExistingSystem<T>();
            ref var systemState = ref world.GetExistingSystemState<T>();
            ref var entityQueries = ref systemState.GetStateQueries();
            Assert.NotNull(system, $"Couldn't find system of type {typeof(T).Name}");
            Assert.NotZero(entityQueries.Length, "System has no queries to test");
            for (var i = 0; i < entityQueries.Length; i++)
            {
                var q = entityQueries[i];
                Assert.NotZero(q.CalculateEntityCount(), $"Query of index {i} has no matches");
            }
        }
    }
}