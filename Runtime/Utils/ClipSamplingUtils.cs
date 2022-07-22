using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DMotion
{
    [BurstCompile]
    internal static class ClipSamplingUtils
    {
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