using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Profiling;
using Unity.Profiling.LowLevel;
using UnityEngine;

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
                    
                    Debug.Log(FixedString.Format("Sampler: {0}, Index {1}, Clip {2}", sampler.Id, i, sampler.Clip.name));
                }
            }
            
            if (activeSamplerCount > 1)
            {
                blender.NormalizeRotations();
            }

            Debug.Log(FixedString.Format("Active Sampler Count: {0}. Total {1}", activeSamplerCount, samplers.Length));

            blender.ApplyBoneHierarchyAndFinish(hierarchyRef.blob);
        }
    }
}