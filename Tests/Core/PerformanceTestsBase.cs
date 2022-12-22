using DMotion.Tests;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.Profiling;
using UnityEngine.TestTools;
using MethodMeasurement = Unity.PerformanceTesting.Measurements.MethodMeasurement;

namespace DMotion.PerformanceTests
{
    public class PerformanceTestsBase : ECSTestBase
    {
        private BurstAndJobConfigsCache cache;
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            cache.Cache();
            PerformanceTestUtils.SetMaxPerformanceBurstParameters();
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            cache.SetCachedValues();
        }

        protected MethodMeasurement DefaultPerformanceMeasure(ProfilerMarker profilerMarker)
        {
            return Measure.Method(() => UpdateWorldWithMarker(profilerMarker))
                .WarmupCount(20)
                .MeasurementCount(50)
                .IterationsPerMeasurement(1);
        }
        
        private void UpdateWorldWithMarker(ProfilerMarker  profilerMarker)
        {
            using var scope = profilerMarker.Auto();
            UpdateWorld();
        }
    }
}