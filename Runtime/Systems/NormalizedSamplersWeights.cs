using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;

namespace DMotion
{
    [BurstCompile]
    internal partial struct NormalizedSamplersWeights : IJobEntity
    {
        internal ProfilerMarker Marker;

        internal void Execute(
            ref DynamicBuffer<ClipSampler> clipSamplers)
        {
            using var scope = Marker.Auto();
            var sumWeights = 0.0f;
            for (var i = 0; i < clipSamplers.Length; i++)
            {
                sumWeights += clipSamplers[i].Weight;
            }

            if (!mathex.aproximately(sumWeights, 1))
            {
                var inverseSumWeights = 1.0f / sumWeights;
                for (var i = 0; i < clipSamplers.Length; i++)
                {
                    var sampler = clipSamplers[i];
                    sampler.Weight *= inverseSumWeights;
                    clipSamplers[i] = sampler;
                }
            }
        }
    }
}