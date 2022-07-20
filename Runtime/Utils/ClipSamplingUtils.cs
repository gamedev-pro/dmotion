using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    internal static class ClipSamplingUtils
    {
        public static BoneTransform SampleAllClips(int boneIndex, in DynamicBuffer<ClipSampler> samplers,
            in ActiveSamplersCount activeSamplersCount)
        {
            if (activeSamplersCount.Value == 0)
            {
                //TODO: assert? Or return reference pose, or boolean. This is not right
                return default;
            }
            
            var firstSampler = samplers[0];
            var bone = ClipSamplingUtils.SampleWeightedFirstIndex(
                boneIndex, ref firstSampler.Clip, firstSampler.NormalizedTime, firstSampler.Weight);
            
            for (byte i = 1; i < activeSamplersCount.Value; i++)
            {
                var sampler = samplers[i];
                ClipSamplingUtils.SampleWeightedNIndex(
                    ref bone, boneIndex, ref sampler.Clip, sampler.NormalizedTime, sampler.Weight);
            }
            return bone;
        }
        
        public static BoneTransform SampleWeightedFirstIndex(int boneIndex, ref SkeletonClip clip, float normalizedTime, float weight)
        {
            var bone = clip.SampleBone(boneIndex, normalizedTime);
            bone.translation *= weight;
            var rot = bone.rotation;
            rot.value *= weight;
            bone.rotation = rot;
            bone.scale *= weight;
            return bone;
        }

        public static void SampleWeightedNIndex(ref BoneTransform bone, int boneIndex, ref SkeletonClip clip, float normalizedTime, float weight)
        {
            var otherBone = clip.SampleBone(boneIndex, normalizedTime);
            bone.translation += otherBone.translation * weight;

            //blends rotation. Negates opposing quaternions to be sure to choose the shortest path
            var otherRot = otherBone.rotation;
            var dot = math.dot(otherRot, bone.rotation);
            if (dot < 0)
            {
                otherRot.value = -otherRot.value;
            }

            var rot = bone.rotation;
            rot.value += otherRot.value * weight;
            bone.rotation = rot;

            bone.scale += otherBone.scale * weight;
        }
    }
}