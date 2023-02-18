using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [BurstCompile]
    [WithNone(typeof(SkeletonRootTag))]
    internal partial struct SampleNonOptimizedBones : IJobEntity
    {
        [ReadOnly] internal BufferLookup<ClipSampler> BfeClipSampler;
        internal ProfilerMarker Marker;

        internal void Execute(
            ref Translation translation,
            ref Rotation rotation,
            ref NonUniformScale scale,
            in BoneOwningSkeletonReference skeletonRef,
            in BoneIndex boneIndex
        )
        {
            using var scope = Marker.Auto();
            var samplers = BfeClipSampler[skeletonRef.skeletonRoot];

            if (samplers.Length > 0 && TryFindFirstActiveSamplerIndex(samplers, out var firstSamplerIndex))
            {
                var firstSampler = samplers[firstSamplerIndex];
                var bone = ClipSamplingUtils.SampleWeightedFirstIndex(
                    boneIndex.index, ref firstSampler.Clip,
                    firstSampler.Time,
                    firstSampler.Weight);

                for (var i = firstSamplerIndex + 1; i < samplers.Length; i++)
                {
                    var sampler = samplers[i];
                    if (!mathex.iszero(sampler.Weight))
                    {
                        ClipSamplingUtils.SampleWeightedNIndex(
                            ref bone, boneIndex.index, ref sampler.Clip,
                            sampler.Time, sampler.Weight);
                    }
                }

                if (samplers.Length - firstSamplerIndex > 1)
                {
                    bone.rotation = math.normalize(bone.rotation);
                }

                translation.Value = bone.translation;
                rotation.Value = bone.rotation;
                scale.Value = bone.scale;
            }
        }

        private bool TryFindFirstActiveSamplerIndex(in DynamicBuffer<ClipSampler> samplers, out byte samplerIndex)
        {
            for (byte i = 0; i < samplers.Length; i++)
            {
                if (!mathex.iszero(samplers[i].Weight))
                {
                    samplerIndex = i;
                    return true;
                }
            }

            samplerIndex = 0;
            return false;
        }
    }
}