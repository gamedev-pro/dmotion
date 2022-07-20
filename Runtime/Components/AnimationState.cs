using System;
using System.Runtime.CompilerServices;
using BovineLabs.Event.Containers;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation
{
    internal struct ClipSampler : IBufferElementData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal ushort ClipIndex;
        internal float NormalizedTime;
        internal float Weight;

        internal ref SkeletonClip Clip => ref Clips.Value.clips[ClipIndex];
    }

    internal struct ActiveSamplersCount : IComponentData
    {
        internal byte Value;
    }
    
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
        internal readonly bool Loop => StateBlob.Loop;

        internal void UpdateSamplers(float dt, float blendWeight, ref DynamicBuffer<ClipSampler> samplers, ref ActiveSamplersCount activeSamplersCount)
        {
            NormalizedTime += dt * Speed;
            switch (Type)
            {
                case StateType.Single:
                    UpdateSamplers_Single(blendWeight, ref samplers, ref activeSamplersCount);
                    break;
                case StateType.LinearBlend:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private readonly void UpdateSamplers_Single(float blendWeight, ref DynamicBuffer<ClipSampler> samplers, ref ActiveSamplersCount activeSamplersCount)
        {
            var samplerIndex = activeSamplersCount.Value;
            
            var sampler = samplers[samplerIndex];
            sampler.Clips = Clips;
            sampler.ClipIndex = AsSingleClip.ClipIndex;
            sampler.NormalizedTime = Loop ? sampler.Clip.LoopToClipTime(NormalizedTime) : NormalizedTime;
            sampler.Weight = blendWeight;
            samplers[samplerIndex] = sampler;
            
            activeSamplersCount.Value++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly float GetNormalizedTimeShifted(float dt)
        {
            return NormalizedTime - dt * Speed;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal readonly void RaiseStateEvents(float dt, Entity animatorEntity, Entity ownerEntity, ref NativeEventStream.ThreadWriter Writer)
        {
            switch (Type)
            {
                case StateType.Single:
                    ref var singleClipState = ref AsSingleClip;
                    var currentNormalizedTime = NormalizedTime;
                    var previousNormalizedTime = GetNormalizedTimeShifted(dt);
                    
                    if (StateBlob.Loop)
                    {
                        ref var clip = ref Clips.Value.clips[singleClipState.ClipIndex];
                        currentNormalizedTime = clip.LoopToClipTime(currentNormalizedTime);
                        previousNormalizedTime = clip.LoopToClipTime(previousNormalizedTime);
                    }

                    ref var clipEvents = ref StateMachineBlob.Value.ClipEvents;
                    for (short i = 0; i < clipEvents.Length; i++)
                    {
                        ref var e = ref clipEvents[i];
                        if (e.ClipIndex == singleClipState.ClipIndex &&
                            e.NormalizedTime >= previousNormalizedTime && e.NormalizedTime <= currentNormalizedTime)
                        {
                            Debug.Log("YO");
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