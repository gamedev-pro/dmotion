using System;
using Latios.Kinemation;
using Unity.Entities;

namespace DOTSAnimation
{
    public enum AnimationSamplerType
    {
        Single,
        LinearBlend
    }
    
    //AnimationState is "polymorphic" (it switches implementation based on AnimationSamplerType)
    //This also means that AnimationState holds data that it doesn't necessarily needs (i.e Blend is only for 1D Blend)
    //I don't like this very much, but the alternative of separating different animation states to different buffers was even messier. At least this option concentrates the switch case mess in one place
    //We could also use https://github.com/PhilSA/PolymorphicStructs (which has code gen for the switch case), but I want to avoid using libraries to solve small things like that
    public partial struct AnimationState : IBufferElementData
    {
        public int StartSamplerIndex;
        public int EndSamplerIndex;
        
        public AnimationSamplerType Type;
        public float TransitionDuration;
        public bool Loop;
        
        //for blend trees only
        public float Blend;
        //

        public void Update(float dt, ref DynamicBuffer<ClipSampler> samplers)
        {
            switch (Type)
            {
                case AnimationSamplerType.Single:
                    Update_SingleClip(dt, ref samplers);
                    break;
                case AnimationSamplerType.LinearBlend:
                    Update_LinearBlend(dt, ref samplers);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public readonly BoneTransform SampleBone(int boneIndex, float timeShift, in DynamicBuffer<ClipSampler> samplers)
        {
            switch (Type)
            {
                case AnimationSamplerType.Single:
                    return SampleBone_SingleClip(boneIndex, timeShift, samplers);
                case AnimationSamplerType.LinearBlend:
                    return SampleBone_LinearBlend(boneIndex, timeShift, samplers);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void SamplePose(ref BufferPoseBlender blender, float timeShift, in DynamicBuffer<ClipSampler> samplers, float blend = 1f)
        {
            switch (Type)
            {
                case AnimationSamplerType.Single:
                    SamplePose_SingleClip(ref blender, timeShift, samplers, blend);
                    break;
                case AnimationSamplerType.LinearBlend:
                    SamplePose_LinearBlend(ref blender, timeShift, samplers, blend);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float GetStateTime(in DynamicBuffer<ClipSampler> samplers)
        {
            switch (Type)
            {
                case AnimationSamplerType.Single:
                    return Time_Single(samplers);
                case AnimationSamplerType.LinearBlend:
                    return Time_LinearBlend(samplers);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public readonly float GetNormalizedStateTime(in DynamicBuffer<ClipSampler> samplers)
        {
            switch (Type)
            {
                case AnimationSamplerType.Single:
                    return NormalizedTime_Single(samplers);
                case AnimationSamplerType.LinearBlend:
                    return NormalizedTime_LinearBlend(samplers);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public readonly int GetActiveSamplerIndex(in DynamicBuffer<ClipSampler> samplers)
        {
            switch (Type)
            {
                case AnimationSamplerType.Single:
                    return GetActiveSamplerIndex_Single(samplers);
                case AnimationSamplerType.LinearBlend:
                    return GetActiveSamplerIndex_LinearBlend(samplers);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void ResetTime(ref DynamicBuffer<ClipSampler> samplers)
        {
            switch (Type)
            {
                case AnimationSamplerType.Single:
                    ResetTime_Single(ref samplers);
                    break;
                case AnimationSamplerType.LinearBlend:
                    ResetTime_LinearBlend(ref samplers);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}