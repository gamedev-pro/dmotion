using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct SampleOptimizedBonesJob : IJobEntity
    {
        internal void Execute(
            ref DynamicBuffer<OptimizedBoneToRoot> boneToRootBuffer,
            in DynamicBuffer<ClipSampler> samplers,
            in ActiveSamplersCount activeSamplersCount,
            in OptimizedSkeletonHierarchyBlobReference hierarchyRef)
        {
            var blender = new BufferPoseBlender(boneToRootBuffer);
            var requiresNormalization = activeSamplersCount.Value > 1;

            for (byte i = 0; i < activeSamplersCount.Value; i++)
            {
                var sampler = samplers[i];
                sampler.Clip.SamplePose(ref blender, sampler.Weight, sampler.NormalizedTime);
            }
            if (requiresNormalization)
            {
                blender.NormalizeRotations();
            }
            blender.ApplyBoneHierarchyAndFinish(hierarchyRef.blob);
        }
        
    }
}