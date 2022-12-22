using DMotion.Tests;
using NUnit.Framework;
using UnityEngine.TestTools;

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
    }
}