using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion.Tests
{
    public static class SystemStateExtensions
    {
        public static ref SystemState GetExistingSystemState<T>(this World world)
            where T : unmanaged, ISystem
        {
            return ref world.Unmanaged.ResolveSystemStateRef(world.GetExistingSystem<T>());
        }

        public static ref UnsafeList<EntityQuery> GetStateQueries(this SystemState state)
        {
            return ref state.EntityQueries;
        }
        
        public static ref UnsafeList<EntityQuery> GetRequiredQueries(this SystemState state)
        {
            return ref state.RequiredEntityQueries;
        }
    }
}