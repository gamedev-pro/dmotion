using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace DOTSAnimation
{
    [BurstCompile]
    [WithNone(typeof(SkeletonRootTag))]
    internal partial struct SampleNonOptimizedBones : IJobEntity
    {
        [ReadOnly] internal BufferFromEntity<ClipSampler> BfeClipSampler;

        internal void Execute(
            ref Translation translation,
            ref Rotation rotation,
            ref NonUniformScale scale,
            in BoneOwningSkeletonReference skeletonRef,
            in BoneIndex boneIndex
        )
        {
            var samplers = BfeClipSampler[skeletonRef.skeletonRoot];

            if (samplers.Length > 0)
            {
                var firstSampler = samplers[0];
                var bone = ClipSamplingUtils.SampleWeightedFirstIndex(
                    boneIndex.index, ref firstSampler.Clip,
                    firstSampler.NormalizedTime,
                    firstSampler.Weight);
                
                for (byte i = 1; i < samplers.Length; i++)
                {
                    var sampler = samplers[i];
                    ClipSamplingUtils.SampleWeightedNIndex(
                        ref bone, boneIndex.index, ref sampler.Clip,
                        sampler.NormalizedTime, sampler.Weight);
                }
                
                translation.Value = bone.translation;
                rotation.Value = bone.rotation;
                scale.Value = bone.scale;
            }
        }
    }
}