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
        [ReadOnly] internal ComponentDataFromEntity<ActiveSamplersCount> CfeActiveSamplerCount;

        internal void Execute(
            ref Translation translation,
            ref Rotation rotation,
            ref NonUniformScale scale,
            in BoneOwningSkeletonReference skeletonRef,
            in BoneIndex boneIndex
        )
        {
            var samplers = BfeClipSampler[skeletonRef.skeletonRoot];
            var activeSamplersCount = CfeActiveSamplerCount[skeletonRef.skeletonRoot];

            if (activeSamplersCount.Value > 0)
            {
                var bone = ClipSamplingUtils.SampleAllClips(boneIndex.index, samplers, activeSamplersCount);
                translation.Value = bone.translation;
                rotation.Value = bone.rotation;
                scale.Value = bone.scale;
            }
        }
    }
}