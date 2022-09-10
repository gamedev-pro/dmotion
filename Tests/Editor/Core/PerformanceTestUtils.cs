using System;
using System.Reflection;
using NUnit.Framework;
using Unity.PerformanceTesting;
using Unity.PerformanceTesting.Runtime;
using Unity.Profiling;
using UnityEngine;

namespace DMotion.Tests
{
    [Serializable]
    public struct TestBenchmarkValue
    {
        [Tooltip("Expected value")] public double Value;

        [UnityEngine.Range(1, 7), Tooltip("Acceptable tolerance +/-")]
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
    }
}