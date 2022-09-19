using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal partial struct NormalizedSamplersWeights : IJobEntity
    {
        internal void Execute(
            ref DynamicBuffer<ClipSampler> clipSamplers)
        {
            var sumWeights = 0.0f;
            for (var i = 0; i < clipSamplers.Length; i++)
            {
                sumWeights += clipSamplers[i].Weight;
            }

            if (!mathex.approximately(sumWeights, 1))
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