using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Collections;
using Unity.Core;
using Unity.Entities;
using UnityEngine;
using UnityEngine.TestTools;

namespace DMotion.Tests
{
    [RequiresPlayMode(false)]
    public abstract class ECSTestBase : ECSTestsFixture
    {
        protected const float defaultDeltaTime = 1.0f / 60.0f;
        private float elapsedTime;
        private NativeArray<SystemHandle> allSystems;
        private BlobAssetStore blobAssetStore;

        public override void Setup()
        {
            base.Setup();
            elapsedTime = Time.time;
            //Create required systems
            {
                var requiredSystemsAttr = GetType().GetCustomAttribute<CreateSystemsForTest>();
                if (requiredSystemsAttr != null)
                {
                    var baseTypeManaged = typeof(SystemBase);
                    var baseType = typeof(ISystem);
                    foreach (var t in requiredSystemsAttr.SystemTypes)
                    {
                        var isValid = baseType.IsAssignableFrom(t) || baseTypeManaged.IsAssignableFrom(t);
                        Assert.IsTrue(isValid,
                            $"Expected {t.Name} to be a subclass of {baseType.Name} or {baseTypeManaged.Name}");
                        world.CreateSystem(t);
                    }

                    allSystems = world.Unmanaged.GetAllSystems(Allocator.Persistent);
                }
            }

            // //Convert entity prefabs
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var convertPrefabField = GetType()
                    .GetFields(bindingFlags)
                    .Where(f => f.GetCustomAttribute<ConvertGameObjectPrefab>() != null).ToArray();
                var createdEntities = new NativeArray<Entity>(convertPrefabField.Length, Allocator.Temp);

                blobAssetStore = new BlobAssetStore(128);
                var conversionWorld = new World("Test Conversion World");

                //instantiate all entities in a conversion word
                for (var i = 0; i < convertPrefabField.Length; i++)
                {
                    var f = convertPrefabField[i];
                    var value = f.GetValue(this);
                    Assert.IsNotNull(value);
                    GameObject go = null;
                    if (value is GameObject g)
                    {
                        go = g;
                    }
                    else if (value is MonoBehaviour mono)
                    {
                        go = mono.gameObject;
                    }

                    Assert.IsNotNull(go);

                    var entity = BakingTestUtils.ConvertGameObject(conversionWorld, go, blobAssetStore);
                    Assert.AreNotEqual(entity, Entity.Null);
                    createdEntities[i] = entity;
                }

                //copy entities from conversionWorld to testWorld
                var outputEntities = new NativeArray<Entity>(createdEntities.Length, Allocator.Temp);
                manager.CopyEntitiesFrom(conversionWorld.EntityManager, createdEntities, outputEntities);

                for (var i = 0; i < convertPrefabField.Length; i++)
                {
                    var f = convertPrefabField[i];
                    var entity = outputEntities[i];
                    var attr = f.GetCustomAttribute<ConvertGameObjectPrefab>();
                    var receiveField = GetType().GetField(attr.ToFieldName, bindingFlags);
                    Assert.IsNotNull(receiveField,
                        $"Couldn't find field to receive entity prefab ({f.Name}, {attr.ToFieldName})");
                    receiveField.SetValue(this, entity);
                }

                conversionWorld.Dispose();
            }
        }

        public override void TearDown()
        {
            base.TearDown();
            if (allSystems.IsCreated)
            {
                allSystems.Dispose();
            }

            if (blobAssetStore.IsCreated)
            {
                blobAssetStore.Dispose();
            }
        }

        protected void UpdateWorld(float deltaTime = defaultDeltaTime)
        {
            if (world != null && world.IsCreated)
            {
                elapsedTime += deltaTime;
                world.SetTime(new TimeData(elapsedTime, deltaTime));
                foreach (var s in allSystems)
                {
                    s.Update(world.Unmanaged);
                }

                //We always want to complete all jobs after update world. Otherwise transformations that test expect to run may not have been run during Assert
                //This is also necessary for performance tests accuracy.
                manager.CompleteAllTrackedJobs();
            }
        }
    }
}