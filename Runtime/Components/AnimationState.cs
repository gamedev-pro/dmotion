using System;
using Latios.Kinemation;
using Unity.Entities;

namespace DOTSAnimation
{
    internal struct AnimationState
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal short StateIndex;
        internal float NormalizedTime;
        internal bool IsValid => StateIndex >= 0;
        internal static AnimationState Null => new() { StateIndex = -1 };
        internal readonly AnimationStateBlob StateBlob => StateMachineBlob.Value.States[StateIndex];
        internal readonly StateType Type => StateBlob.Type;
        
        internal void Update(float dt)
        {
            NormalizedTime += dt * Speed;
        }
        
        internal readonly BoneTransform SampleBone(float time, int boneIndex)
        {
            switch (Type)
            {
                case StateType.Single:
                    return SampleBone_SingleClip(time, boneIndex);
                case StateType.LinearBlend:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal readonly void SamplePose(float time, float blend, ref BufferPoseBlender blender)
        {
            switch (Type)
            {
                case StateType.Single:
                    SamplePose_SingleClip(time, blend, ref blender);
                    break;
                case StateType.LinearBlend:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal readonly float Speed
        {
            get
            {
                switch (Type)
                {
                    case StateType.Single:
                        return StateMachineBlob.Value.SingleClipStates[StateIndex].Speed;
                    case StateType.LinearBlend:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal readonly float GetNormalizedTimeShifted(float dt)
        {
            return NormalizedTime - dt * Speed;
        }

        internal readonly ref SingleClipStateBlob AsSingleClip => ref StateMachineBlob.Value.SingleClipStates[StateIndex];

        internal readonly ref SkeletonClip GetClip(int index)
        {
            return ref Clips.Value.clips[index];
        }
        internal readonly BoneTransform SampleBone_SingleClip(float time, int boneIndex)
        {
            ref var singleClipState = ref AsSingleClip;
            ref var clip = ref GetClip(singleClipState.ClipIndex);
            var normalizedTime = singleClipState.Loop ? clip.LoopToClipTime(time) : time;
            return clip.SampleBone(boneIndex, normalizedTime);
        }

        internal readonly void SamplePose_SingleClip(float time, float blend, ref BufferPoseBlender blender)
        {
            ref var singleClipState = ref AsSingleClip;
            ref var clip = ref GetClip(singleClipState.ClipIndex);
            var normalizedTime = singleClipState.Loop ? clip.LoopToClipTime(time) : time;
            clip.SamplePose(ref blender, blend, normalizedTime);
        }
    }
}