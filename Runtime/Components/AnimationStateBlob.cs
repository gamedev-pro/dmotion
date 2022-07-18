using Latios.Kinemation;
using Unity.Entities;

namespace DOTSAnimation
{
    public enum StateType
    {
        Single,
        LinearBlend
    }

    internal struct AnimationStateBlob
    {
        internal StateType Type;
        internal short StateIndex;
    }
    
    internal struct SingleClipStateBlob
    {
        internal ushort ClipIndex;
        internal bool Loop;
        internal float Speed;
    }

    // internal struct LinearBlendStateBlob : IAnimationState
    // {
    //     public int StartSamplerIndex;
    //     public int EndSamplerIndex;
    //     public float Blend;
    //     
    //     public void Update(float dt)
    //     {
    //         LinearBlendSampling.UpdateSamplers(StartSamplerIndex, EndSamplerIndex, Blend, dt);
    //     }
    //     
    //     public BoneTransform SampleBone(int boneIndex, float timeShift, in DynamicBuffer<ClipSampler> samplers)
    //     {
    //         return LinearBlendSampling.SampleBone(boneIndex, timeShift, samplers, StartSamplerIndex, EndSamplerIndex);
    //     }
    //
    //     public void SamplePose(ref BufferPoseBlender blender, float timeShift, in DynamicBuffer<ClipSampler> samplers, float blend = 1f)
    //     {
    //         LinearBlendSampling.SamplePose(ref blender, timeShift, samplers, StartSamplerIndex, EndSamplerIndex, blend);
    //     }
    //
    //     public float GetNormalizedStateTime(in DynamicBuffer<ClipSampler> samplers)
    //     { 
    //         LinearBlendSampling.FindSamplers(samplers, StartSamplerIndex, EndSamplerIndex, Blend, out var firstSamplerIndex, out var secondSamplerIndex);
    //         var duration = LinearBlendSampling.GetBlendDuration(samplers, firstSamplerIndex, secondSamplerIndex);
    //         var time = LinearBlendSampling.GetBlendTime(samplers, firstSamplerIndex, secondSamplerIndex);
    //         return AnimationUtils.CalculateNormalizedTime(time, duration);
    //     }
    //
    //     public int GetActiveSamplerIndex(in DynamicBuffer<ClipSampler> samplers)
    //     {
    //         var maxWeightIndex = -1;
    //         var maxWeight = -1.0f;
    //         for (var i = StartSamplerIndex; i <= EndSamplerIndex; i++)
    //         {
    //             if (samplers[i].Weight > maxWeight)
    //             {
    //                 maxWeight = samplers[i].Weight;
    //                 maxWeightIndex = i;
    //             }
    //         }
    //         return maxWeightIndex;
    //     }
    //
    //     public void ResetTime(ref DynamicBuffer<ClipSampler> samplers)
    //     {
    //         for (var i = StartSamplerIndex; i <= EndSamplerIndex; i++)
    //         {
    //             var s = samplers[i];
    //             s.Time = 0;
    //             samplers[i] = s;
    //         }
    //     }
    //
    //     public float GetStateTime(in DynamicBuffer<ClipSampler> samplers)
    //     {
    //         LinearBlendSampling.FindSamplers(samplers, StartSamplerIndex, EndSamplerIndex, Blend, out var firstSamplerIndex, out var secondSamplerIndex);
    //         return LinearBlendSampling.GetBlendTime(samplers, firstSamplerIndex, secondSamplerIndex);
    //     }
    // }
}