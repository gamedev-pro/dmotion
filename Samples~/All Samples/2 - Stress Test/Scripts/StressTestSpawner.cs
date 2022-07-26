using System;
using Unity.Entities;

namespace DMotion.StressTest
{
    [Serializable]
    [GenerateAuthoringComponent]
    public struct StressTestSpawner : IComponentData
    {
        public Entity SkeletonPrefab;
        public int SkeletonsCount;
        public float Spacing;
    }
}