using Latios.Kinemation;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    public static class AnimationUtils
    {
        public static BoneTransform SampleBlended(int boneIndex, ref SkeletonClip from, float fromClipTime, ref SkeletonClip to, float toClipTime, float blend)
        {
            var fromTransform = from.SampleBone(boneIndex, fromClipTime);
            var toTransform = to.SampleBone(boneIndex, toClipTime);

            fromTransform.translation = math.lerp(fromTransform.translation, toTransform.translation, blend);
            fromTransform.rotation = math.nlerp(fromTransform.rotation, toTransform.rotation, blend);
            fromTransform.scale = math.lerp(fromTransform.scale, toTransform.scale, blend);
            return fromTransform;
        }
        public static BoneTransform SampleBlended(int boneIndex, float time, ref SkeletonClip from, ref SkeletonClip to, float blend)
        {
            return SampleBlended(boneIndex, ref from, from.LoopToClipTime(time), ref to, to.LoopToClipTime(time), blend);
        }

        public static float4x4 ToBTRMatrix(this BoneTransform boneTransform, int boneIndex, in NativeArray<float4x4> bones, in OptimizedSkeletonHierarchyBlobReference hierarchyBlob)
        {
            var boneToParent = float4x4.TRS(boneTransform.translation, boneTransform.rotation, boneTransform.scale);
            var parentIndex = hierarchyBlob.blob.Value.parentIndices[boneIndex];
            return parentIndex >= 0 ? math.mul(bones[parentIndex], boneToParent) : boneToParent;
        }
        public static NativeArray<float4x4> AsBTRMatrixArray(this DynamicBuffer<OptimizedBoneToRoot> boneToRootBuffer)
        {
            return boneToRootBuffer.Reinterpret<float4x4>().AsNativeArray();
        }

        public static float CalculateNormalizedTime(float time, float duration)
        {
            var wrappedTime  = math.fmod(time, duration);
            wrappedTime       += math.select(0f, duration, wrappedTime < 0f);
            return wrappedTime;
        }
    }
}