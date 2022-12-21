using DMotion.Tests;
using NUnit.Framework;

namespace DMotion.PerformanceTests
{
    public class PerformanceTestsFixture : ECSTestBase
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