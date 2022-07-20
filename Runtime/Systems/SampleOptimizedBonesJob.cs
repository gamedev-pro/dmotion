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
            in OptimizedSkeletonHierarchyBlobReference hierarchyRef)
        {
            var blender = new BufferPoseBlender(boneToRootBuffer);
            var requiresNormalization = samplers.Length > 1;

            for (byte i = 0; i < samplers.Length; i++)
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