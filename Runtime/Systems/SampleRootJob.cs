using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    [WithAll(typeof(SkeletonRootTag))]
    internal partial struct SampleRootJob : IJobEntity
    {
        internal void Execute(
            ref RootDeltaTranslation rootDeltaTranslation,
            ref RootDeltaRotation rootDeltaRotation,
            in DynamicBuffer<ClipSampler> samplers
        )
        {
            rootDeltaTranslation.Value = 0;
            rootDeltaRotation.Value = quaternion.identity;
            if (samplers.Length > 0 && TryGetFirstSamplerIndex(samplers, out var startIndex))
            {
                var firstSampler = samplers[startIndex];
                var root = ClipSamplingUtils.SampleWeightedFirstIndex(
                    0, ref firstSampler.Clip,
                    firstSampler.NormalizedTime,
                    firstSampler.Weight);
                
                var previousRoot = ClipSamplingUtils.SampleWeightedFirstIndex(
                    0, ref firstSampler.Clip,
                    firstSampler.PreviousNormalizedTime,
                    firstSampler.Weight);

                for (var i = startIndex + 1; i < samplers.Length; i++)
                {
                    var sampler = samplers[i];
                    if (ShouldIncludeSampler(sampler))
                    {
                        ClipSamplingUtils.SampleWeightedNIndex(
                            ref root, 0, ref sampler.Clip,
                            sampler.NormalizedTime, sampler.Weight);
                        
                        ClipSamplingUtils.SampleWeightedNIndex(
                            ref previousRoot, 0, ref sampler.Clip,
                            sampler.PreviousNormalizedTime, sampler.Weight);
                    }
                }
                    
                rootDeltaTranslation.Value = root.translation - previousRoot.translation;
                rootDeltaRotation.Value = mathex.delta(root.rotation, previousRoot.rotation);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ShouldIncludeSampler(in ClipSampler sampler)
        {
            //Since we're calculating deltas, we need to avoid the loop point (the character would teleport back to the initial root position)
            return sampler.NormalizedTime - sampler.PreviousNormalizedTime > 0;
        }

        private static bool TryGetFirstSamplerIndex(in DynamicBuffer<ClipSampler> samplers, out byte startIndex)
        {
            for (byte i = 0; i < samplers.Length; i++)
            {
                if (ShouldIncludeSampler(samplers[i]))
                {
                    startIndex = i;
                    return true;
                }
            }
            startIndex = 0;
            return false;
        }
    }
}