using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.PerformanceTesting;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Profiling;

namespace DMotion.Tests
{
    public class MethodMeasurement
    {
        private const int k_MeasurementCount = 9;
        private const int k_MinMeasurementTimeMs = 100;
        private const int k_MinWarmupTimeMs = 100;
        private const int k_ProbingMultiplier = 4;
        private const int k_MaxIterations = 10000;
        private readonly Action m_Action;
        private readonly List<SampleGroup> m_SampleGroups = new List<SampleGroup>();
        private readonly Recorder m_GCRecorder;

        private Action m_Setup;
        private Action m_Cleanup;
        private SampleGroup m_SampleGroup;
        private SampleGroup m_SampleGroupGC;
        private int m_WarmupCount;
        private int m_MeasurementCount;
        private int m_IterationCount = 1;
        private bool m_GC;
        private readonly Stopwatch m_Watch;

        public MethodMeasurement(Action action) : this(SampleUnit.Millisecond, false, action)
        {
        }

        public MethodMeasurement(SampleUnit unit, bool increaseIsBetter, Action action)
        {
            m_Action = action;
            m_GCRecorder = Recorder.Get("GC.Alloc");
            m_GCRecorder.enabled = false;
            m_Watch = Stopwatch.StartNew();

            m_SampleGroup = new SampleGroup("Time", unit, increaseIsBetter);
            m_SampleGroupGC = new SampleGroup("Time.GC()", SampleUnit.Undefined, increaseIsBetter);
        }

        public MethodMeasurement ProfilerMarkers(SampleUnit unit, params ProfilerMarker[] markers)
        {
            return ProfilerMarkers(unit, markers.Select(m => m.GetNameSlow()).ToArray());
        }
        public MethodMeasurement ProfilerMarkers(SampleUnit unit, params string[] profilerMarkerNames)
        {
            if (profilerMarkerNames == null) return this;
            foreach (var marker in profilerMarkerNames)
            {
                var sampleGroup = new SampleGroup(marker, unit, false);
                sampleGroup.GetRecorder();
                sampleGroup.GetRecorder().enabled = false;
                m_SampleGroups.Add(sampleGroup);
            }

            return this;
        }

        public MethodMeasurement SampleGroup(string name)
        {
            m_SampleGroup = new SampleGroup(name, m_SampleGroup.Unit, m_SampleGroup.IncreaseIsBetter);
            m_SampleGroupGC = new SampleGroup(name + ".GC()", m_SampleGroupGC.Unit, m_SampleGroupGC.IncreaseIsBetter);
            return this;
        }

        public MethodMeasurement WarmupCount(int count)
        {
            m_WarmupCount = count;
            return this;
        }

        public MethodMeasurement IterationsPerMeasurement(int count)
        {
            m_IterationCount = count;
            return this;
        }

        public MethodMeasurement MeasurementCount(int count)
        {
            m_MeasurementCount = count;
            return this;
        }

        public MethodMeasurement CleanUp(Action action)
        {
            m_Cleanup = action;
            return this;
        }

        public MethodMeasurement SetUp(Action action)
        {
            m_Setup = action;
            return this;
        }

        public MethodMeasurement GC()
        {
            m_GC = true;
            return this;
        }

        public void Run()
        {
            if (m_MeasurementCount > 0)
            {
                Warmup(m_WarmupCount);
                RunForIterations(m_IterationCount, m_MeasurementCount);
                return;
            }

            var iterations = Probing();
            RunForIterations(iterations, k_MeasurementCount);
        }

        private void RunForIterations(int iterations, int measurements)
        {
            EnableMarkers();
            for (var j = 0; j < measurements; j++)
            {
                var executionTime = iterations == 1 ? ExecuteSingleIteration() : ExecuteForIterations(iterations);
                Measure.Custom(m_SampleGroup, executionTime / iterations);
            }

            DisableAndMeasureMarkers();
        }

        private void EnableMarkers()
        {
            foreach (var sampleGroup in m_SampleGroups)
            {
                sampleGroup.GetRecorder().enabled = true;
            }
        }

        private void DisableAndMeasureMarkers()
        {
            foreach (var sampleGroup in m_SampleGroups)
            {
                sampleGroup.GetRecorder().enabled = false;
                var sample = NanoSecondsToSampleUnit(sampleGroup.Unit, sampleGroup.GetRecorder().elapsedNanoseconds);
                var blockCount = sampleGroup.GetRecorder().sampleBlockCount;
                if (blockCount == 0) continue;
                Measure.Custom(sampleGroup, (double)sample / blockCount);
            }
        }

        private int Probing()
        {
            var executionTime = 0.0D;
            var iterations = 1;

            while (executionTime < k_MinWarmupTimeMs)
            {
                executionTime = m_Watch.Elapsed.TotalMilliseconds;
                Warmup(iterations);
                executionTime = m_Watch.Elapsed.TotalMilliseconds - executionTime;

                if (executionTime < k_MinWarmupTimeMs)
                {
                    iterations *= k_ProbingMultiplier;
                }
            }

            if (iterations == 1)
            {
                ExecuteActionWithCleanupSetup();
                ExecuteActionWithCleanupSetup();

                return 1;
            }

            var deisredIterationsCount =
                Mathf.Clamp((int)(k_MinMeasurementTimeMs * iterations / executionTime), 1, k_MaxIterations);

            return deisredIterationsCount;
        }

        private void Warmup(int iterations)
        {
            for (var i = 0; i < iterations; i++)
            {
                ExecuteActionWithCleanupSetup();
            }
        }

        private double ExecuteActionWithCleanupSetup()
        {
            m_Setup?.Invoke();
            var executionTime = m_Watch.Elapsed.TotalMilliseconds;
            m_Action.Invoke();
            executionTime = m_Watch.Elapsed.TotalMilliseconds - executionTime;
            m_Cleanup?.Invoke();

            return executionTime;
        }

        private long NanoSecondsToSampleUnit(SampleUnit unit, long nanoSeconds)
        {
            switch (unit)
            {
                case SampleUnit.Nanosecond:
                    return nanoSeconds;
                case SampleUnit.Microsecond:
                    return nanoSeconds / 1000;
                case SampleUnit.Millisecond:
                    return nanoSeconds / 1_000_000;
                case SampleUnit.Second:
                    return nanoSeconds / 1_000_000_000;
                case SampleUnit.Byte:
                case SampleUnit.Kilobyte:
                case SampleUnit.Megabyte:
                case SampleUnit.Gigabyte:
                case SampleUnit.Undefined:
                    throw new Exception("Unsupported");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double MillisecondsToSampleUnit(SampleUnit unit, double millseconds)
        {
            switch (unit)
            {
                case SampleUnit.Nanosecond:
                    return millseconds * 1_000_000;
                case SampleUnit.Microsecond:
                    return millseconds * 1000;
                case SampleUnit.Millisecond:
                    return millseconds;
                case SampleUnit.Second:
                    return millseconds / 1000;
                case SampleUnit.Byte:
                case SampleUnit.Kilobyte:
                case SampleUnit.Megabyte:
                case SampleUnit.Gigabyte:
                case SampleUnit.Undefined:
                    throw new Exception("Unsupported");
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private double ExecuteSingleIteration()
        {
            if (m_GC) StartGCRecorder();
            m_Setup?.Invoke();

            var executionTime = m_Watch.Elapsed.TotalMilliseconds;
            m_Action.Invoke();
            executionTime = m_Watch.Elapsed.TotalMilliseconds - executionTime;

            m_Cleanup?.Invoke();
            if (m_GC) EndGCRecorderAndMeasure(1);
            return MillisecondsToSampleUnit(m_SampleGroup.Unit, executionTime);
        }

        private double ExecuteForIterations(int iterations)
        {
            if (m_GC) StartGCRecorder();
            var executionTime = 0.0D;

            if (m_Cleanup != null || m_Setup != null)
            {
                for (var i = 0; i < iterations; i++)
                {
                    executionTime += ExecuteActionWithCleanupSetup();
                }
            }
            else
            {
                executionTime = m_Watch.Elapsed.TotalMilliseconds;
                for (var i = 0; i < iterations; i++)
                {
                    m_Action.Invoke();
                }

                executionTime = m_Watch.Elapsed.TotalMilliseconds - executionTime;
            }

            if (m_GC) EndGCRecorderAndMeasure(iterations);
            return MillisecondsToSampleUnit(m_SampleGroup.Unit, executionTime);
        }

        private void StartGCRecorder()
        {
            System.GC.Collect();

            m_GCRecorder.enabled = false;
            m_GCRecorder.enabled = true;
        }

        private void EndGCRecorderAndMeasure(int iterations)
        {
            m_GCRecorder.enabled = false;

            Measure.Custom(m_SampleGroupGC, (double)m_GCRecorder.sampleBlockCount / iterations);
        }
    }
}