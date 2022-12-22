using UnityEngine;

namespace DMotion.Tests
{
    [CreateAssetMenu(menuName = "DMotion/Tests/Performance Benchmarks")]
    public class PerformanceTestBenchmarksPerMachine : ScriptableObject
    {
        public MachineSpecificPerformanceBenchmarks[] MachineBenchmarks;
        //
    }
}