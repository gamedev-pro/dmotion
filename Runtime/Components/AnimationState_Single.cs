using System.Collections.Generic;
using Latios.Kinemation;
using Unity.Entities;

namespace DOTSAnimation
{
    public partial struct AnimationState
    {
        private void Update_SingleClip(float dt, ref DynamicBuffer<ClipSampler> samplers)
        {
            var s = samplers[StartSamplerIndex];
            s.Time += dt * s.Speed;
            samplers[StartSamplerIndex] = s;
        }
        
        private readonly BoneTransform SampleBone_SingleClip(int boneIndex, float timeShift, in DynamicBuffer<ClipSampler> samplers)
        {
            var sampler = samplers[StartSamplerIndex];
            var time = sampler.Time + timeShift * sampler.Speed;
            var normalizedTime = Loop ? sampler.Clip.LoopToClipTime(time) : time;
            return sampler.Clip.SampleBone(boneIndex, normalizedTime);
        }

        private readonly float NormalizedTime_Single(in DynamicBuffer<ClipSampler> samplers)
        {
            var s = samplers[StartSamplerIndex];
            return s.Clip.LoopToClipTime(s.Time);
        }
        
        private void ResetTime_Single(ref DynamicBuffer<ClipSampler> samplers)
        {
            var s = samplers[StartSamplerIndex];
            s.Time = 0;
            samplers[StartSamplerIndex] = s;
        }

        private float Time_Single(in DynamicBuffer<ClipSampler> samplers)
        {
            var s = samplers[StartSamplerIndex];
            return s.Time;
        }

        private readonly int GetActiveSamplerIndex_Single(DynamicBuffer<ClipSampler> samplers)
        {
            return StartSamplerIndex;
        }
    }
}
