using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using BovineLabs.Event.Containers;
using BovineLabs.Event.Jobs;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    [BurstCompile]
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
        internal readonly ref SingleClipStateBlob AsSingleClip => ref StateMachineBlob.Value.SingleClipStates[StateIndex];
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly float GetNormalizedTimeShifted(float dt)
        {
            return NormalizedTime - dt * Speed;
        }

        internal readonly BoneTransform SampleBone_SingleClip(float time, int boneIndex)
        {
            ref var singleClipState = ref AsSingleClip;
            ref var clip = ref Clips.Value.clips[singleClipState.ClipIndex];
            var normalizedTime = singleClipState.Loop ? clip.LoopToClipTime(time) : time;
            return clip.SampleBone(boneIndex, normalizedTime);
        }

        internal readonly void SamplePose_SingleClip(float time, float blend, ref BufferPoseBlender blender)
        {
            ref var singleClipState = ref AsSingleClip;
            ref var clip = ref Clips.Value.clips[singleClipState.ClipIndex];
            var normalizedTime = singleClipState.Loop ? clip.LoopToClipTime(time) : time;
            clip.SamplePose(ref blender, blend, normalizedTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly float GetLoopClipTime(float time)
        {
            switch (Type)
            {
                case StateType.Single:
                    ref var singleClipState = ref AsSingleClip;
                    ref var clip = ref Clips.Value.clips[singleClipState.ClipIndex];
                    return singleClipState.Loop ? clip.LoopToClipTime(time) : time;
                case StateType.LinearBlend:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly void RaiseStateEvents(float dt, Entity animatorEntity, Entity ownerEntity, ref NativeEventStream.ThreadWriter Writer)
        {
            switch (Type)
            {
                case StateType.Single:
                    ref var clipEvents = ref StateMachineBlob.Value.ClipEvents[AsSingleClip.ClipIndex];
                    var currentNormalizedTime = GetLoopClipTime(NormalizedTime);
                    var previousNormalizedTime = GetLoopClipTime(GetNormalizedTimeShifted(dt));
                    for (short i = 0; i < clipEvents.Events.Length; i++)
                    {
                        ref var e = ref clipEvents.Events[i];
                        if (e.NormalizedTime >= previousNormalizedTime && e.NormalizedTime <= currentNormalizedTime)
                        {
                            Writer.Write(new RaisedAnimationEvent()
                            {
                                EventHash = e.EventHash,
                                AnimatorEntity = animatorEntity,
                                AnimatorOwner = ownerEntity,
                            });
                        }
                    }
                    break;
                case StateType.LinearBlend:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}