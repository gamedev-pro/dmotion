using System;
using System.Reflection;
using NUnit.Framework;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Runtime;
using Unity.Profiling;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.TestTools;

namespace DMotion.Tests
{
    [Serializable]
    public struct TestBenchmarkValue
    {
        [Tooltip("Expected value")] public double Value;

        [UnityEngine.Range(1, 20), Tooltip("Acceptable tolerance +/-")]
        public int Tolerance;

        public static TestBenchmarkValue DefaultTolerance => new TestBenchmarkValue() { Tolerance = 10 };

        public double Min => Value * (1 - Tolerance / 100f);
        public double Max => Value * (1 + Tolerance / 100f);
    }

    [Serializable]
    public struct PerformanceTestBenchmark
    {
        public int Count;
        public TestBenchmarkValue Median;
        public TestBenchmarkValue Max;
        public TestBenchmarkValue Min;
    }

    [Serializable]
    public struct MachineSpecificPerformanceBenchmarks
    {
        public string MachineName;
        public PerformanceTestBenchmark[] Benchmarks;
    }

    public static class PerformanceTestUtils
    {
        public static string GetNameSlow(this ProfilerMarker marker)
        {
            var method = marker.GetType().GetMethod("GetName", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.IsNotNull(method, "Couldn't find GetName method");
            var args = new object[1];
            method.Invoke(marker, args);
            var name = (string)args[0];
            Assert.NotNull(name);
            Assert.IsNotEmpty(name);
            return name;
        }

        public static void AssertWithinBenchmark(this PerformanceTestBenchmark benchmark)
        {
            static void AssertTestResult(string propertyName, in TestBenchmarkValue benchmarkValue, double actual)
            {
                Assert.NotZero(actual, "Received 0 for actual values. This is probably no correct");
                Assert.GreaterOrEqual(actual, benchmarkValue.Min, $"{propertyName}: Actual {actual} below Min value {benchmarkValue.Min} (Expected: {benchmarkValue.Value}, Tolerance: {benchmarkValue.Tolerance}%. Did performance improve?");
                Assert.LessOrEqual(actual, benchmarkValue.Max, $"{propertyName}: Actual {actual} above Max acceptable value {benchmarkValue.Max} (Expected: {benchmarkValue.Value}, Tolerance: {benchmarkValue.Tolerance}%. Did performance decrease?");
            }
            
            Assert.Greater(PerformanceTest.Active.SampleGroups.Count, 0, "Not Sample groups found to benchmark against");
            var sampleGroup = PerformanceTest.Active.SampleGroups[0];
            sampleGroup.UpdateStatistics();
            AssertTestResult(nameof(benchmark.Median), benchmark.Median, sampleGroup.Median);
            AssertTestResult(nameof(benchmark.Min), benchmark.Min, sampleGroup.Min);
            AssertTestResult(nameof(benchmark.Max), benchmark.Max, sampleGroup.Max);
        }

        [MenuItem("Tools/DMotion/Set Performance Burst Parameters")]
        public static void SetMaxPerformanceBurstParameters()
        {
            JobsUtility.JobCompilerEnabled = true;
            JobsUtility.JobDebuggerEnabled = false;
            NativeLeakDetection.Mode = NativeLeakDetectionMode.Disabled;
            // CompilationPipeline.codeOptimization = CodeOptimization.Release;
            
            BurstCompiler.Options.EnableBurstCompilation = true;
            BurstCompiler.Options.EnableBurstDebug = false;
            BurstCompiler.Options.EnableBurstSafetyChecks = false;
            BurstCompiler.Options.ForceEnableBurstSafetyChecks = false;
            BurstCompiler.Options.EnableBurstCompileSynchronously = true;
            Coverage.enabled = false;
        }

        [MenuItem("Tools/DMotion/Set Debug Burst Parameters")]
        public static void SetDebugBustParameters()
        {
            JobsUtility.JobCompilerEnabled = true;
            JobsUtility.JobDebuggerEnabled = true;
            NativeLeakDetection.Mode = NativeLeakDetectionMode.EnabledWithStackTrace;
            // CompilationPipeline.codeOptimization = CodeOptimization.Debug;
            
            BurstCompiler.Options.EnableBurstDebug = true;
            BurstCompiler.Options.EnableBurstCompilation = false;
            BurstCompiler.Options.EnableBurstSafetyChecks = true;
            BurstCompiler.Options.ForceEnableBurstSafetyChecks = false;
            BurstCompiler.Options.EnableBurstCompileSynchronously = false;
            Coverage.enabled = true;
        }
    }
}