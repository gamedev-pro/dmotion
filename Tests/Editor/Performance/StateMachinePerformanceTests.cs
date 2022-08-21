using DMotion.Tests;
using NUnit.Framework;
using Unity.Entities;
using Unity.PerformanceTesting;
using UnityEngine;

namespace DMotion.PerformanceTests
{
    public class StateMachinePerformanceTests : PerformanceTestsFixture
    {
        [SerializeField, ConvertGameObjectPrefab(nameof(noSkeletonPrefabEntity))]
        private GameObject noSkeletonPrefab;

        private Entity noSkeletonPrefabEntity;
        
        static int[] testValues = { 100, 1000, 10_000, 100_000 };

        [Test, Performance]
        public void StateMachineOnly_StressTest([ValueSource(nameof(testValues))] int count)
        {
            world.CreateSystem<AnimationStateMachineSystem>();
            InstantiateEntitiesAndMeasure(count, noSkeletonPrefabEntity);
        }
        
        [Test, Performance]
        public void StateMachine_WithParameterUpdates_StressTest([ValueSource(nameof(testValues))] int count)
        {
            world.CreateSystem<AnimationStateMachineSystem>();
            world.CreateSystem<UpdateStateMachines>();
            
            InstantiateEntitiesAndMeasure(count, noSkeletonPrefabEntity);
        }

        private void InstantiateEntitiesAndMeasure(int count, Entity prefab)
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

            Measure.Method(() => { UpdateWorld(); })
                .WarmupCount(2)
                .MeasurementCount(10)
                .IterationsPerMeasurement(1)
                .Run();
        }
    }
}