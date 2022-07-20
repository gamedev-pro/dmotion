using System;
using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LoopToClipTime()
        {
            NormalizedTime = Clip.LoopToClipTime(NormalizedTime);
            PreviousNormalizedTime = Clip.LoopToClipTime(PreviousNormalizedTime);
        }
    }

    internal struct ActiveSamplersCount : IComponentData
    {
        internal byte Value;

        internal int Take()
        {
            return Value++;
        }
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

        internal readonly ref SingleClipStateBlob AsSingleClip =>
            ref StateMachineBlob.Value.SingleClipStates[StateBlob.StateIndex];

        internal readonly ref LinearBlendStateBlob AsLinearBlend =>
            ref StateMachineBlob.Value.LinearBlendStates[StateBlob.StateIndex];

        internal readonly float Speed => StateBlob.Speed;
        internal readonly bool Loop => StateBlob.Loop;

        internal void UpdateSamplers(float dt, float blendWeight, in DynamicBuffer<BlendParameter> blendParameters,
            ref DynamicBuffer<ClipSampler> samplers, ref ActiveSamplersCount activeSamplersCount)
        {
            var prevNormalizedTime = NormalizedTime;
            NormalizedTime += dt * Speed;
            switch (Type)
            {
                case StateType.Single:
                    UpdateSamplers_Single(prevNormalizedTime, blendWeight, ref samplers, ref activeSamplersCount);
                    break;
                case StateType.LinearBlend:
                    UpdateSamplers_LinearBlend(prevNormalizedTime, blendWeight, blendParameters, ref samplers, ref activeSamplersCount);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private readonly void UpdateSamplers_Single(float prevNormalizedTime, float blendWeight,
            ref DynamicBuffer<ClipSampler> samplers, ref ActiveSamplersCount activeSamplersCount)
        {
            ref var singleClip = ref AsSingleClip;
            var samplerIndex = activeSamplersCount.Take();

            var sampler = samplers[samplerIndex];
            sampler.Clips = Clips;
            sampler.ClipIndex = singleClip.ClipIndex;
            sampler.PreviousNormalizedTime = prevNormalizedTime;
            sampler.NormalizedTime = NormalizedTime;
            sampler.Weight = blendWeight;
            if (Loop)
            {
                sampler.LoopToClipTime();
            }
            
            samplers[samplerIndex] = sampler;
        }

        private void UpdateSamplers_LinearBlend(
            float prevNormalizedTime, float blendWeight,
            in DynamicBuffer<BlendParameter> blendParameters,
            ref DynamicBuffer<ClipSampler> samplers, ref ActiveSamplersCount activeSamplersCount)
        {
            ref var linearBlendState = ref AsLinearBlend;
            ref var sortedClips = ref linearBlendState.ClipSortedByThreshold;
            var startIndex = 0;
            var endIndex = sortedClips.Length - 1;
            var blendRatio = blendParameters[linearBlendState.BlendParameterIndex].Value;

            //we assume thresholds are sorted here
            blendRatio = math.clamp(blendRatio, sortedClips[startIndex].Threshold, sortedClips[endIndex].Threshold);
            //find clip tuple to be blended
            var firstClipIndex = -1;
            for (var i = startIndex + 1; i <= endIndex; i++)
            {
                var currentClip = sortedClips[i];
                var prevClip = sortedClips[i - 1];
                if (blendRatio >= prevClip.Threshold && blendRatio <= currentClip.Threshold)
                {
                    firstClipIndex = i - 1;
                    break;
                }
            }

            var firstClip = sortedClips[firstClipIndex];
            var secondClip = sortedClips[firstClipIndex + 1];

            var firstSamplerIndex = activeSamplersCount.Take();
            var secondSamplerIndex = activeSamplersCount.Take();

            var firstSampler = samplers[firstSamplerIndex];
            var secondSampler = samplers[secondSamplerIndex];

            //Update sampler clip references
            {
                firstSampler.Clips = Clips;
                firstSampler.ClipIndex = firstClip.ClipIndex;
                
                secondSampler.Clips = Clips;
                secondSampler.ClipIndex = secondClip.ClipIndex;
            }

            //Update sampler weights
            {
                var t = (blendRatio - firstClip.Threshold) / (secondClip.Threshold - firstClip.Threshold);
                firstSampler.Weight = (1 - t)*blendWeight;
                secondSampler.Weight = t*blendWeight;
            }

            //Update sampler times
            {
                var loopDuration = firstSampler.Clip.duration * firstSampler.Weight +
                                   secondSampler.Clip.duration * secondSampler.Weight;

                var invLoopDuration = 1.0f / loopDuration;

                var firstSamplerRatio = firstSampler.Clip.duration * invLoopDuration;
                var secondSamplerRatio = secondSampler.Clip.duration * invLoopDuration;

                firstSampler.NormalizedTime = NormalizedTime * firstSamplerRatio;
                firstSampler.PreviousNormalizedTime = prevNormalizedTime * firstSamplerRatio;
                
                secondSampler.NormalizedTime = NormalizedTime * secondSamplerRatio;
                secondSampler.PreviousNormalizedTime = prevNormalizedTime * secondSamplerRatio;

                if (Loop)
                {
                    firstSampler.LoopToClipTime();
                    secondSampler.LoopToClipTime();
                }
            }

            samplers[firstSamplerIndex] = firstSampler;
            samplers[secondSamplerIndex] = secondSampler;
        }
    }
}