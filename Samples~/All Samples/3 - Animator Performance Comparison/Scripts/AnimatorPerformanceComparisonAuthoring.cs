using System.Collections.Generic;
using DMotion.StressTest;
using Unity.Entities;
using UnityEngine;

namespace DMotion.ComparisonTest
{
    public class AnimatorPerformanceComparisonAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    {
        public GameObject EntityPrefab;
        public GameObject AnimatorPrefab;
        public int Count = 1000;
        public float Spacing = 1;
        public bool UseEntity;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            if (UseEntity)
            {
                dstManager.AddComponentData(entity, new StressTestSpawner()
                {
                    SkeletonPrefab = conversionSystem.GetPrimaryEntity(EntityPrefab),
                    SkeletonsCount = Count,
                    Spacing = Spacing
                });
            }
            else
            {
                dstManager.AddComponentData(entity, new AnimatorSpawner()
                {
                    AnimatorPrefab = AnimatorPrefab,
                    Count = Count,
                    Spacing = Spacing
                });           
            }
        }

        public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
        {
            //no need to add the gameobject prefab here
            referencedPrefabs.Add(EntityPrefab);
        }
    }
}