using System.Linq;
using System.Reflection;
using NUnit.Framework;
using Unity.Core;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Tests
{
    public abstract class ECSTestBase : ECSTestsFixture
    {
        private const float defaultDeltaTime = 1.0f / 60.0f;
        private float elapsedTime;
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
                    var baseType = typeof(SystemBase);
                    foreach (var t in requiredSystemsAttr.SystemTypes)
                    {
                        Assert.IsTrue(baseType.IsAssignableFrom(t),
                            $"Expected {t.Name} to be a subclass of {baseType.Name}");
                        world.CreateSystem(t);
                    }
                }
            }

            blobAssetStore = new BlobAssetStore(128);
            // //Convert entity prefabs
            {
                var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
                var convertPrefabField = GetType()
                    .GetFields(bindingFlags)
                    .Where(f => f.GetCustomAttribute<ConvertGameObjectPrefab>() != null);
                foreach (var f in convertPrefabField)
                {
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

                    var entity = BakingTestUtils.ConvertGameObject(world, go, blobAssetStore);
                    Assert.AreNotEqual(entity, Entity.Null);
            
                    var attr = f.GetCustomAttribute<ConvertGameObjectPrefab>();
                    var receiveField = GetType().GetField(attr.ToFieldName, bindingFlags);
                    Assert.IsNotNull(receiveField, $"Couldn't find field to receive entity prefab ({f.Name}, {attr.ToFieldName})");
                    receiveField.SetValue(this, entity);
                }
            }
        }

        public override void TearDown()
        {
            base.TearDown();
            blobAssetStore.Dispose();
        }

        protected void UpdateWorld(float deltaTime = defaultDeltaTime, bool completeAllJobs = true)
        {
            if (world != null && world.IsCreated)
            {
                elapsedTime += deltaTime;
                world.SetTime(new TimeData(elapsedTime, deltaTime));
                foreach (var s in world.Systems)
                {
                    s.Update();
                }

                if (completeAllJobs)
                {
                    manager.CompleteAllTrackedJobs();
                }
            }
            
        }
    }
}