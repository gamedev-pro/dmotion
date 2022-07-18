using System;
using System.Runtime.CompilerServices;
using BovineLabs.Event.Containers;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

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
        internal readonly float Speed => StateBlob.Speed;

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
            var normalizedTime = StateBlob.Loop ? clip.LoopToClipTime(time) : time;
            return clip.SampleBone(boneIndex, normalizedTime);
        }

        internal readonly void SamplePose_SingleClip(float time, float blend, ref BufferPoseBlender blender)
        {
            ref var singleClipState = ref AsSingleClip;
            ref var clip = ref Clips.Value.clips[singleClipState.ClipIndex];
            var normalizedTime = StateBlob.Loop ? clip.LoopToClipTime(time) : time;
            clip.SamplePose(ref blender, blend, normalizedTime);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly void RaiseStateEvents(float dt, Entity animatorEntity, Entity ownerEntity, ref NativeEventStream.ThreadWriter Writer)
        {
            switch (Type)
            {
                case StateType.Single:
                    ref var singleClipState = ref AsSingleClip;
                    ref var clipEvents = ref StateMachineBlob.Value.ClipEvents[singleClipState.ClipIndex];
                    var currentNormalizedTime = NormalizedTime;
                    var previousNormalizedTime = GetNormalizedTimeShifted(dt);
                    
                    if (StateBlob.Loop)
                    {
                        ref var clip = ref Clips.Value.clips[singleClipState.ClipIndex];
                        currentNormalizedTime = clip.LoopToClipTime(currentNormalizedTime);
                        previousNormalizedTime = clip.LoopToClipTime(previousNormalizedTime);
                    }
                    
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