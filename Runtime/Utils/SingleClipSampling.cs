using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    public static class SingleClipSampling
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static BoneTransform SampleBoneBlended(
            int boneIndex, float blend, float timeShift,
            in AnimationState currentState, in AnimationState nextState,
            in DynamicBuffer<ClipSampler> samplers)
        {
            var current = currentState.SampleBone(boneIndex, timeShift, samplers);
            var next = nextState.SampleBone(boneIndex, timeShift, samplers);

            current.translation = math.lerp(current.translation, next.translation, blend);
            current.rotation = math.nlerp(current.rotation, next.rotation, blend);
            current.scale = math.lerp(current.scale, next.scale, blend);
            return current;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SamplePoseBlended(
            ref BufferPoseBlender blender, float blend, float timeShift,
            in AnimationState currentState, in AnimationState nextState,
            in DynamicBuffer<ClipSampler> samplers)
        {
            currentState.SamplePose(ref blender, timeShift, samplers, 1f - blend);
            nextState.SamplePose(ref blender, timeShift, samplers, blend);
        }
    }
}