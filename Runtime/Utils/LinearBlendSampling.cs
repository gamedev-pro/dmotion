using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    // [BurstCompile]
    // public static class LinearBlendSampling
    // {
    //     public static BoneTransform SampleBone(
    //         int boneIndex,
    //         float timeShift,
    //         in DynamicBuffer<ClipSampler> samplers,
    //         int startIndex, int endIndex)
    //     {
    //         var startSampler = samplers[startIndex];
    //         var startSamplerTime = startSampler.Time + timeShift * startSampler.Speed;
    //         var bone = LinearBlendSampling.SampleWeightedFirstIndex(boneIndex, ref startSampler.Clip, startSamplerTime, startSampler.Weight);
    //         for (var i = startIndex + 1; i <= endIndex; i++)
    //         {
    //             var s = samplers[i];
    //             var t = s.Time + timeShift * s.Speed;
    //             LinearBlendSampling.SampleWeightedNIndex(ref bone, boneIndex, ref s.Clip, t, s.Weight);
    //         }
    //         return bone;
    //     }
    //
    //     public static void SamplePose(
    //         ref BufferPoseBlender blender,
    //         float timeShift,
    //         in DynamicBuffer<ClipSampler> samplers,
    //         int startIndex, int endIndex, float blend = 1f)
    //     {
    //         for (var i = startIndex; i <= endIndex; i++)
    //         {
    //             var s = samplers[i];
    //             var t = s.Time + timeShift * s.Speed;
    //             t = s.Clip.LoopToClipTime(t);
    //             s.Clip.SamplePose(ref blender, s.Weight * blend, t);
    //         }
    //     }
    //
    //     public static void FindSamplers(
    //         in DynamicBuffer<ClipSampler> samplers,
    //         int startIndex, int endIndex,
    //         float blendRatio, out int firstSamplerIndex, out int secondSamplerIndex)
    //     {
    //         firstSamplerIndex = -1;
    //         secondSamplerIndex = -1;
    //         blendRatio = math.clamp(blendRatio, samplers[startIndex].Threshold, samplers[endIndex].Threshold);
    //         for (var i = startIndex + 1; i <= endIndex; i++)
    //         {
    //             var currentSampler = samplers[i];
    //             var prevSampler = samplers[i - 1];
    //             if (blendRatio >= prevSampler.Threshold && blendRatio <= currentSampler.Threshold)
    //             {
    //                 firstSamplerIndex = i - 1;
    //                 secondSamplerIndex = i;
    //                 break;
    //             }
    //         }
    //     }
    //
    //     public static float GetBlendDuration(in DynamicBuffer<ClipSampler> samplers, int firstSamplerIndex, int secondSamplerIndex)
    //     {
    //         var firstSampler = samplers[firstSamplerIndex];
    //         var secondSampler = samplers[secondSamplerIndex];
    //         
    //         return firstSampler.Clip.duration * firstSampler.Weight +
    //                            secondSampler.Clip.duration * secondSampler.Weight;
    //     }
    //     
    //     public static float GetBlendTime(in DynamicBuffer<ClipSampler> samplers, int firstSamplerIndex, int secondSamplerIndex)
    //     {
    //         var firstSampler = samplers[firstSamplerIndex];
    //         var secondSampler = samplers[secondSamplerIndex];
    //         
    //         return firstSampler.Time * firstSampler.Weight +
    //                            secondSampler.Time * secondSampler.Weight;
    //     }
    //
    //     public static void UpdateSamplers(
    //         ref DynamicBuffer<ClipSampler> samplers,
    //         int startIndex,
    //         int endIndex,
    //         float blendRatio,
    //         float dt)
    //     {
    //         blendRatio = math.clamp(blendRatio, samplers[startIndex].Threshold, samplers[endIndex].Threshold);
    //         //find clip tuple to be blended
    //         var firstSamplerIndex = -1;
    //         for (var i = startIndex + 1; i <= endIndex; i++)
    //         {
    //             var currentSampler = samplers[i];
    //             var prevSampler = samplers[i - 1];
    //             if (blendRatio >= prevSampler.Threshold && blendRatio <= currentSampler.Threshold)
    //             {
    //                 firstSamplerIndex = i - 1;
    //                 break;
    //             }
    //         }
    //         
    //         var firstSampler = samplers[firstSamplerIndex];
    //         var secondSampler = samplers[firstSamplerIndex + 1];
    //         
    //         //Update clip weights
    //         {
    //             for (var i = startIndex; i <= endIndex; i++)
    //             {
    //                 var sampler = samplers[i];
    //                 sampler.Weight = 0;
    //                 samplers[i] = sampler;
    //             }
    //             
    //             var t = (blendRatio - firstSampler.Threshold) / (secondSampler.Threshold - firstSampler.Threshold);
    //             firstSampler.Weight = 1 - t;
    //             secondSampler.Weight = t;
    //             samplers[firstSamplerIndex] = firstSampler;
    //             samplers[firstSamplerIndex + 1] = secondSampler;
    //         }
    //         
    //         //Update clip speeds to match duration
    //         {
    //             var loopDuration = firstSampler.Clip.duration * firstSampler.Weight +
    //                                secondSampler.Clip.duration * secondSampler.Weight;
    //
    //             var invLoopDuration = 1.0f / loopDuration;
    //             for (var i = startIndex; i <= endIndex; i++)
    //             {
    //                 var sampler = samplers[i];
    //                 sampler.Speed = sampler.Clip.duration * invLoopDuration;
    //                 samplers[i] = sampler;
    //             }
    //         }
    //         
    //         //Update clip times
    //         for (var i = startIndex; i <= endIndex; i++)
    //         {
    //             var c = samplers[i];
    //             c.Time += dt * c.Speed;
    //             samplers[i] = c;
    //         }
    //     }
    //     
    //     public static BoneTransform SampleWeightedFirstIndex(int boneIndex, ref SkeletonClip clip, float clipTime, float clipWeight)
    //     {
    //         var t = clip.LoopToClipTime(clipTime);
    //         var bone = clip.SampleBone(boneIndex, t);
    //         bone.translation *= clipWeight;
    //         var rot = bone.rotation;
    //         rot.value *= clipWeight;
    //         bone.rotation = rot;
    //         bone.scale *= clipWeight;
    //         return bone;
    //     }
    //
    //     public static void SampleWeightedNIndex(ref BoneTransform bone, int boneIndex, ref SkeletonClip clip, float clipTime,
    //         float clipWeight)
    //     {
    //         var t = clip.LoopToClipTime(clipTime);
    //         var otherBone = clip.SampleBone(boneIndex, t);
    //         bone.translation += otherBone.translation * clipWeight;
    //         
    //         //blends rotation. Negates opposing quaternions to be sure to choose the shortest path
    //         var otherRot = otherBone.rotation;
    //         var dot = math.dot(otherRot, bone.rotation);
    //         if (dot < 0)
    //         {
    //             otherRot.value = -otherRot.value;
    //         }
    //         var rot = bone.rotation;
    //         rot.value += otherRot.value * clipWeight;
    //         bone.rotation = rot;
    //
    //         bone.scale += otherBone.scale * clipWeight;
    //     }
    // }
}