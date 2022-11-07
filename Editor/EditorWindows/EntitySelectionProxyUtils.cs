using Unity.Entities;
using Unity.Entities.Editor;

namespace DMotion.Editor
{
    internal static class EntitySelectionProxyUtils
    {
        internal static bool HasComponent<T>(this EntitySelectionProxy proxy)
            where T : IComponentData
        {
            return proxy.Exists && proxy.World.EntityManager.HasComponent<T>(proxy.Entity);
        }

        internal static T GetComponent<T>(this EntitySelectionProxy proxy)
            where T : struct, IComponentData
        {
            return proxy.World.EntityManager.GetComponentData<T>(proxy.Entity);
        }

        internal static T GetManagedComponent<T>(this EntitySelectionProxy proxy)
            where T : class, IComponentData
        {
            return proxy.World.EntityManager.GetComponentData<T>(proxy.Entity);
        }

        internal static DynamicBuffer<T> GetBuffer<T>(this EntitySelectionProxy proxy)
            where T : struct, IBufferElementData
        {
            return proxy.World.EntityManager.GetBuffer<T>(proxy.Entity);
        }
    }
}