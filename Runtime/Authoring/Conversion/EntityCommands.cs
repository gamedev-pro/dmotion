using System;
using Unity.Entities;

namespace DMotion.Authoring
{
    /// <summary>
    /// This is a wrapper for the EntityManager, IBaker, and ECB
    /// </summary>
    public struct EntityCommands
    {
        private enum EntityCommandsType
        {
            EntityManager,
            Ecb,
            Baker,
        }

        private EntityManager entityManager;
        private EntityCommandBuffer ecb;
        private EntityCommandsType type;
        private IBaker baker;

        public static implicit operator EntityCommands(IBaker baker)
        {
            return new EntityCommands() { type = EntityCommandsType.Baker, baker = baker };
        }

        public static implicit operator EntityCommands(EntityManager entityManager)
        {
            return new EntityCommands() { type = EntityCommandsType.EntityManager, entityManager = entityManager };
        }

        public static implicit operator EntityCommands(EntityCommandBuffer ecb)
        {
            return new EntityCommands() { type = EntityCommandsType.Ecb, ecb = ecb };
        }

        public DynamicBuffer<T> AddBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData
        {
            return type switch
            {
                EntityCommandsType.EntityManager => entityManager.AddBuffer<T>(entity),
                EntityCommandsType.Ecb => ecb.AddBuffer<T>(entity),
                EntityCommandsType.Baker => baker.AddBuffer<T>(entity),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void AddComponent<T>(Entity entity, T c)
            where T : unmanaged, IComponentData
        {
            switch (type)
            {
                case EntityCommandsType.EntityManager:
                    entityManager.AddComponentData(entity, c);
                    break;
                case EntityCommandsType.Ecb:
                    ecb.AddComponent(entity, c);
                    break;
                case EntityCommandsType.Baker:
                    baker.AddComponent(entity, c);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity)
            where T : unmanaged, IBufferElementData
        {
            return type switch
            {
                EntityCommandsType.EntityManager => entityManager.GetBuffer<T>(entity),
                EntityCommandsType.Ecb => throw new Exception("Ecb doesn't support GetBuffer"),
                EntityCommandsType.Baker => throw new Exception("IBaker doesn't support GetBuffer"),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public void AddComponentObject<T>(Entity entity, T c)
            where T : class, IComponentData
        {
            switch (type)
            {
                case EntityCommandsType.EntityManager:
                    entityManager.AddComponentObject(entity, c);
                    break;
                case EntityCommandsType.Ecb:
                    ecb.AddComponent(entity, c);
                    break;
                case EntityCommandsType.Baker:
                    baker.AddComponentObject(entity, c);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}