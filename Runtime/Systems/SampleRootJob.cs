using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    
    [BurstCompile]
    [WithAll(typeof(SkeletonRootTag))]
    internal partial struct SampleRootJob : IJobEntity
    {
        internal void Execute(
            ref RootTranslation rootTranslation,
            ref RootRotation rootRotation,
            ref RootPreviousTranslation rootPreviousTranslation,
            ref RootPreviousRotation rootPreviousRotation,
            ref RootDeltaTranslation rootDeltaTranslation,
            ref RootDeltaRotation rootDeltaRotation,
            in DynamicBuffer<ClipSampler> samplers,
            in ActiveSamplersCount activeSamplersCount
        )
        {
            rootPreviousTranslation.Value = rootTranslation.Value;
            rootPreviousRotation.Value = rootRotation.Value;
            if (activeSamplersCount.Value > 0)
            {
                var root = ClipSamplingUtils.SampleAllClips(0, samplers, activeSamplersCount);
                rootTranslation.Value = root.translation;
                rootRotation.Value = root.rotation;
                
                rootDeltaTranslation.Value = rootTranslation.Value - rootPreviousTranslation.Value;
                rootDeltaRotation.Value = mathex.delta(rootRotation.Value, rootPreviousRotation.Value);
            }
        }
    }
}