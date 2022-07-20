using System;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    internal struct ClipSampler : IBufferElementData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal ushort ClipIndex;
        internal float PreviousNormalizedTime;
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
            var prevNormalizedTime = NormalizedTime;
            NormalizedTime += dt * Speed;
            switch (Type)
            {
                case StateType.Single:
                    UpdateSamplers_Single(prevNormalizedTime, blendWeight, ref samplers, ref activeSamplersCount);
                    break;
                case StateType.LinearBlend:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private readonly void UpdateSamplers_Single(float prevNormalizedTime, float blendWeight, ref DynamicBuffer<ClipSampler> samplers, ref ActiveSamplersCount activeSamplersCount)
        {
            var samplerIndex = activeSamplersCount.Value;
            
            var sampler = samplers[samplerIndex];
            sampler.Clips = Clips;
            sampler.ClipIndex = AsSingleClip.ClipIndex;
            sampler.PreviousNormalizedTime = Loop ? sampler.Clip.LoopToClipTime(prevNormalizedTime) : prevNormalizedTime;
            sampler.NormalizedTime = Loop ? sampler.Clip.LoopToClipTime(NormalizedTime) : NormalizedTime;
            sampler.Weight = blendWeight;
            samplers[samplerIndex] = sampler;
            
            activeSamplersCount.Value++;
        }
    }
}