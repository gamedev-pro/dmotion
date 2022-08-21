using DMotion.Tests;
using NUnit.Framework;
using Unity.Entities;
using Unity.PerformanceTesting;
using UnityEngine;

namespace DMotion.PerformanceTests
{
    [CreateSystemsForTest(typeof(AnimationStateMachineSystem), typeof(UpdateStateMachines))]
    public class StateMachinePerformanceTests : PerformanceTestsFixture
    {
        [SerializeField, ConvertGameObjectPrefab(nameof(noSkeletonPrefabEntity))]
        private GameObject noSkeletonPrefab;
        
        [SerializeField, ConvertGameObjectPrefab(nameof(skeletonPrefabEntity))]
        private GameObject skeletonPrefab;

        private Entity noSkeletonPrefabEntity;
        private Entity skeletonPrefabEntity;
        
        static int[] testValues = { 1000, 10_000, 100_000 };
        
        [Test, Performance]
        public void AverageUpdateTime([ValueSource(nameof(testValues))] int count)
        {
            InstantiateEntities(count, skeletonPrefabEntity);
            Measure.Method(() => UpdateWorld())
                .WarmupCount(2)
                .MeasurementCount(10)
                .IterationsPerMeasurement(1)
                .Run();
        }
        
        [Test, Performance]
        public void JobMarkers([ValueSource(nameof(testValues))] int count)
        {
            InstantiateEntities(count, skeletonPrefabEntity);
            using var markersMeasurement = Measure.ProfilerMarkers(
                    AnimationStateMachineSystem.Marker_UpdateStateMachineJob.GetNameSlow(),
                    AnimationStateMachineSystem.Marker_SampleOptimizedBonesJob.GetNameSlow(),
                    UpdateStateMachines.Marker.GetNameSlow()
                );
            UpdateWorld();
        }

        private void InstantiateEntities(int count, Entity prefab)
        {
            for (var i = 0; i < count; i++)
            {
                var e = manager.Instantiate(prefab);
                
                Assert.IsTrue(manager.HasComponent<LinearBlendDirection>(e));
                Assert.IsTrue(manager.HasComponent<PlayOneShotRequest>(e));
                Assert.IsTrue(manager.HasComponent<BlendParameter>(e));
                Assert.IsTrue(manager.HasComponent<BoolParameter>(e));
                Assert.IsTrue(manager.HasComponent<StressTestOneShotClip>(e));
            }

            UpdateWorld();
        }
    }
}