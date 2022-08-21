using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;

namespace DMotion.PerformanceTests
{
    public class PerformanceTestsFixture : ECSTestsFixture
    {
        private bool prevJobCompilerEnabled;
        private bool prevEnableBurstCompilation;
        private bool prevEnableSafetyChecks;
        private bool prevForceEnableSafetyChecks;
        private bool prevCompileSynchronously;
        private NativeLeakDetectionMode prevLeakDetectionMode;
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            prevJobCompilerEnabled = JobsUtility.JobCompilerEnabled;
            JobsUtility.JobCompilerEnabled = true;

            prevLeakDetectionMode = NativeLeakDetection.Mode;
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;

            prevEnableBurstCompilation = BurstCompiler.Options.EnableBurstCompilation;
            prevEnableSafetyChecks = BurstCompiler.Options.EnableBurstSafetyChecks;
            prevForceEnableSafetyChecks = BurstCompiler.Options.ForceEnableBurstSafetyChecks;
            prevCompileSynchronously = BurstCompiler.Options.EnableBurstCompileSynchronously;

            BurstCompiler.Options.EnableBurstCompilation = true;
            BurstCompiler.Options.EnableBurstSafetyChecks = false;
            BurstCompiler.Options.ForceEnableBurstSafetyChecks = false;
            BurstCompiler.Options.EnableBurstCompileSynchronously = true;
        }

        [TearDown]
        public override void TearDown()
        {
            base.TearDown();
            JobsUtility.JobCompilerEnabled = prevJobCompilerEnabled;
            NativeLeakDetection.Mode = prevLeakDetectionMode;

            BurstCompiler.Options.EnableBurstCompilation = prevEnableBurstCompilation;
            BurstCompiler.Options.EnableBurstSafetyChecks = prevEnableSafetyChecks;
            BurstCompiler.Options.ForceEnableBurstSafetyChecks = prevForceEnableSafetyChecks;
            BurstCompiler.Options.EnableBurstCompileSynchronously = prevCompileSynchronously;
        }
    }
}