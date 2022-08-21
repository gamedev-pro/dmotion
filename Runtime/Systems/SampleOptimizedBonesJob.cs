using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;
using Unity.Profiling.LowLevel;

namespace DMotion
{
    [BurstCompile]
    internal partial struct SampleOptimizedBonesJob : IJobEntity
    {
        internal ProfilerMarker Marker;
        
        internal void Execute(
            ref DynamicBuffer<OptimizedBoneToRoot> boneToRootBuffer,
            in DynamicBuffer<ClipSampler> samplers,
            in OptimizedSkeletonHierarchyBlobReference hierarchyRef)
        {
            using var scope = Marker.Auto();
            var blender = new BufferPoseBlender(boneToRootBuffer);
            var activeSamplerCount = 0;

            for (byte i = 0; i < samplers.Length; i++)
            {
                var sampler = samplers[i];
                if (!mathex.iszero(sampler.Weight))
                {
                    activeSamplerCount++;
                    sampler.Clip.SamplePose(ref blender, sampler.Weight, sampler.Time);
                }
            }
            
            if (activeSamplerCount > 1)
            {
                blender.NormalizeRotations();
            }

            blender.ApplyBoneHierarchyAndFinish(hierarchyRef.blob);
        }
    }
}