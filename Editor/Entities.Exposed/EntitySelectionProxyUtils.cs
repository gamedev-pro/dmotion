using Unity.Entities;
using Unity.Entities.Editor;
using UnityEditor;

namespace DMotion.Editor
{
    public class EntitySelectionProxyWrapper
    {
        internal EntitySelectionProxy Value;
        public bool Exists => Value is { Exists: true };
        public Entity Entity => Value.Entity;
        public World World => Value.World;

        public EntitySelectionProxyWrapper(UnityEngine.Object obj)
        {
            Value = (EntitySelectionProxy)obj;
        }
    }
    
    public static class EntitySelectionProxyUtils
    {
        public static bool IsEntitySelectionProxy(this UnityEngine.Object obj)
        {
            return obj is EntitySelectionProxy;
        }

        public static bool TryExtractEntitySelectionProxy(out EntitySelectionProxyWrapper proxyWrapper)
        {
            var proxy = Selection.activeContext as EntitySelectionProxy;
            if (proxy == null)
            {
                proxy = Selection.activeObject as EntitySelectionProxy;
            }

            if (proxy != null)
            {
                proxyWrapper = new EntitySelectionProxyWrapper(proxy);
                return true;
            }

            proxyWrapper = default;
            return false;
        }
        
        public static bool HasComponent<T>(this EntitySelectionProxyWrapper proxy)
            where T : IComponentData
        {
            return proxy.Exists && proxy.World.EntityManager.HasComponent<T>(proxy.Entity);
        }

        public static bool TryGetManagedComponent<T>(this EntitySelectionProxyWrapper proxy, out T c)
            where T : class, IComponentData, new()
        {
            if (proxy.Exists && proxy.World.EntityManager.HasComponent<T>(proxy.Entity))
            {
                c = proxy.GetManagedComponent<T>();
                return true;
            }

            c = default;
            return false;
        }

        public static T GetComponent<T>(this EntitySelectionProxyWrapper proxy)
            where T : unmanaged, IComponentData
        {
            return proxy.World.EntityManager.GetComponentData<T>(proxy.Entity);
        }

        public static T GetManagedComponent<T>(this EntitySelectionProxyWrapper proxy)
            where T : class, IComponentData, new()
        {
            return proxy.World.EntityManager.GetComponentData<T>(proxy.Entity);
        }

        public static DynamicBuffer<T> GetBuffer<T>(this EntitySelectionProxyWrapper proxy)
            where T : unmanaged, IBufferElementData
        {
            return proxy.World.EntityManager.GetBuffer<T>(proxy.Entity);
        }
    }
}