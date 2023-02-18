using Unity.Burst;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEditor.Compilation;
using UnityEngine.TestTools;

namespace DMotion.PerformanceTests
{
    public struct BurstAndJobConfigsCache
    {
        private bool prevJobCompilerEnabled;
        private CodeOptimization prevEngineCodeOptimization;
        private NativeLeakDetectionMode prevLeakDetectionMode;
        private bool coverageWasEnabled;
        
        private bool prevEnableBurstCompilation;
        private bool prevEnableBurstDebug;
        private bool prevEnableSafetyChecks;
        private bool prevForceEnableSafetyChecks;
        private bool prevCompileSynchronously;

        public void Cache()
        {
            //Jobs
            prevJobCompilerEnabled = JobsUtility.JobCompilerEnabled;
            prevLeakDetectionMode = NativeLeakDetection.Mode;
            
            //engine optimization
            prevEngineCodeOptimization = CompilationPipeline.codeOptimization;
            
            //code coverage
            coverageWasEnabled = Coverage.enabled;
            
            //burst
            prevEnableBurstCompilation = BurstCompiler.Options.EnableBurstCompilation;
            prevEnableBurstDebug = BurstCompiler.Options.EnableBurstDebug;
            prevEnableSafetyChecks = BurstCompiler.Options.EnableBurstSafetyChecks;
            prevForceEnableSafetyChecks = BurstCompiler.Options.ForceEnableBurstSafetyChecks;
            prevCompileSynchronously = BurstCompiler.Options.EnableBurstCompileSynchronously;
        }

        public void SetCachedValues()
        {
            //Jobs
            JobsUtility.JobCompilerEnabled = prevJobCompilerEnabled;
            NativeLeakDetection.Mode = prevLeakDetectionMode;
            
            //engine optimziation
            CompilationPipeline.codeOptimization = prevEngineCodeOptimization;
            
            //code coverage
            Coverage.enabled = coverageWasEnabled;

            //burst
            BurstCompiler.Options.EnableBurstCompilation = prevEnableBurstCompilation;
            BurstCompiler.Options.EnableBurstDebug = prevEnableBurstDebug;
            BurstCompiler.Options.EnableBurstSafetyChecks = prevEnableSafetyChecks;
            BurstCompiler.Options.ForceEnableBurstSafetyChecks = prevForceEnableSafetyChecks;
            BurstCompiler.Options.EnableBurstCompileSynchronously = prevCompileSynchronously;
        }
    }
}