using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    internal static class SingleClipSampling
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static BoneTransform SampleBoneBlended(
            in AnimationState currentStateBlob, float currentStatNormalizedTime,
            in AnimationState nextStateBlob, float nextStateNormalizedTime,
            float blend, int boneIndex)
        {
            var current = currentStateBlob.SampleBone(currentStatNormalizedTime, boneIndex);
            var next = nextStateBlob.SampleBone(nextStateNormalizedTime, boneIndex);

            current.translation = math.lerp(current.translation, next.translation, blend);
            current.rotation = math.nlerp(current.rotation, next.rotation, blend);
            current.scale = math.lerp(current.scale, next.scale, blend);

            return current;
        }
    }
}