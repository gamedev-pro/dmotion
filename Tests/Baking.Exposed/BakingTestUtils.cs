using System.Reflection;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Tests
{
    public static class BakingTestUtils
    {
        public static void BakeGameObjects(World conversionWorld, GameObject[] rootGameObjects,
            BlobAssetStore blobAssetStore)
        {
            var settings = new BakingSettings(BakingUtility.BakingFlags.AssignName, blobAssetStore);
            BakingUtility.BakeGameObjects(conversionWorld, rootGameObjects, settings);
        }

        public static Entity ConvertGameObject(World world, GameObject go, BlobAssetStore store)
        {
            BakeGameObjects(world, new []{go}, store);
            return GetEntityForGameObject(world, go);
        }

        public static Entity GetEntityForGameObject(World world, GameObject go)
        {
            var bakingSystem = world.GetOrCreateSystemManaged<BakingSystem>();
            return bakingSystem.GetBakeEntityData().GetEntity(go);
        }

        internal static BakedEntityData GetBakeEntityData(this BakingSystem bakingSystem)
        {
            var reflectedBakingEntitiesField =
                typeof(BakingSystem).GetField("_BakedEntities", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.IsNotNull(reflectedBakingEntitiesField, "Couldn't find BakedEntities field using reflection");
            return (BakedEntityData)reflectedBakingEntitiesField.GetValue(bakingSystem);
        }
    }
}