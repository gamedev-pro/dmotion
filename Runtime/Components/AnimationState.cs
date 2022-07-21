using System;
using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    public struct PlayOneShotRequest : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEvents;
        internal short ClipIndex;
        internal float NormalizedTransitionDuration;
        internal float Speed;

        public bool IsValid => ClipIndex >= 0 && Clips.IsCreated;

        public static PlayOneShotRequest Null => new() { ClipIndex = -1 };

        public PlayOneShotRequest(BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents, int clipIndex,
            float normalizedTransitionDuration = 0.15f,
            float speed = 1)
        {
            Clips = clips;
            ClipEvents = clipEvents;
            ClipIndex = (short) clipIndex;
            NormalizedTransitionDuration = normalizedTransitionDuration;
            Speed = speed;
        }
    }
    
    internal struct OneShotState : IComponentData
    {
        internal short SamplerIndex;
        internal float NormalizedTransitionDuration;
        internal float Speed;

        internal bool IsValid => SamplerIndex >= 0;
        internal static OneShotState Null => new() { SamplerIndex = -1 };
    }
    
    internal struct ClipSampler : IBufferElementData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEventsBlob;
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

    [BurstCompile]
    internal struct AnimationState
    {
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal short StateIndex;
        internal float NormalizedTime;
        internal byte StartSamplerIndex;
        
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

        internal byte ClipCount
        {
            get
            {
                switch (Type)
                {
                    case StateType.Single:
                        return 1;
                    case StateType.LinearBlend:
                        return (byte)AsLinearBlend.ClipSortedByThreshold.Length;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        internal void Initialize(BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            StartSamplerIndex = (byte)samplers.Length;
            switch (Type)
            {
                case StateType.Single:
                    Initialize_Single(clips, clipEvents, ref samplers);
                    break;
                case StateType.LinearBlend:
                    Initialize_LinearBlend(clips, clipEvents, ref samplers);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Initialize_LinearBlend(
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var linearBlendState = ref AsLinearBlend;
            samplers.Length += linearBlendState.ClipSortedByThreshold.Length;
            for (var i = 0; i < linearBlendState.ClipSortedByThreshold.Length; i++)
            {
                var clip = linearBlendState.ClipSortedByThreshold[i];
                samplers[StartSamplerIndex + i] = new ClipSampler()
                {
                    ClipIndex = clip.ClipIndex,
                    Clips = clips,
                    ClipEventsBlob = clipEvents,
                    PreviousNormalizedTime = 0,
                    NormalizedTime = 0,
                    Weight = 0
                };
            }
        }

        private void Initialize_Single(
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var singleClip = ref AsSingleClip;
            samplers.Add(new ClipSampler()
            {
                ClipIndex = singleClip.ClipIndex,
                Clips = clips,
                ClipEventsBlob = clipEvents,
                PreviousNormalizedTime = 0,
                NormalizedTime = 0,
                Weight = 0
            });
        }

        internal void UpdateSamplers(float dt, float blendWeight, in DynamicBuffer<BlendParameter> blendParameters,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            NormalizedTime += dt * Speed;
            switch (Type)
            {
                case StateType.Single:
                    UpdateSamplers_Single(dt, blendWeight, ref samplers);
                    break;
                case StateType.LinearBlend:
                    UpdateSamplers_LinearBlend(dt, blendWeight, blendParameters, ref samplers);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private readonly void UpdateSamplers_Single(float dt, float blendWeight,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var singleClip = ref AsSingleClip;
            var samplerIndex = StartSamplerIndex;

            var sampler = samplers[samplerIndex];
            sampler.Weight = blendWeight;

            sampler.PreviousNormalizedTime = sampler.NormalizedTime;
            sampler.NormalizedTime += dt * Speed;
            if (Loop)
            {
                sampler.LoopToClipTime();
            }

            samplers[samplerIndex] = sampler;
        }

        private void UpdateSamplers_LinearBlend(
            float dt, float blendWeight,
            in DynamicBuffer<BlendParameter> blendParameters,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            if (mathex.iszero(blendWeight))
            {
                return;
            }
            ref var linearBlendState = ref AsLinearBlend;
            ref var sortedClips = ref linearBlendState.ClipSortedByThreshold;
            var startIndex = StartSamplerIndex;
            var endIndex = StartSamplerIndex + sortedClips.Length - 1;

            var blendRatio = blendParameters[linearBlendState.BlendParameterIndex].Value;

            //we assume thresholds are sorted here
            blendRatio = math.clamp(blendRatio, sortedClips[0].Threshold, sortedClips[sortedClips.Length - 1].Threshold);
            
            //find clip tuple to be blended
            var firstClipIndex = -1;
            var secondClipIndex = -1;
            for (var i = 1; i < sortedClips.Length; i++)
            {
                var currentClip = sortedClips[i];
                var prevClip = sortedClips[i - 1];
                if (blendRatio >= prevClip.Threshold && blendRatio <= currentClip.Threshold)
                {
                    firstClipIndex = i - 1;
                    secondClipIndex = i;
                    break;
                }
            }

            var firstClip = sortedClips[firstClipIndex];
            var secondClip = sortedClips[secondClipIndex];

            var firstSamplerIndex = startIndex + firstClipIndex;
            var secondSamplerIndex = startIndex + secondClipIndex;

            var firstSampler = samplers[firstSamplerIndex];
            var secondSampler = samplers[secondSamplerIndex];
            
            //Update clip weights
            {
                for (var i = startIndex; i <= endIndex; i++)
                {
                    var sampler = samplers[i];
                    sampler.Weight = 0;
                    samplers[i] = sampler;
                }

                var t = (blendRatio - firstClip.Threshold) / (secondClip.Threshold - firstClip.Threshold);
                firstSampler.Weight = (1 - t)*blendWeight;
                secondSampler.Weight = t*blendWeight;
                samplers[firstSamplerIndex] = firstSampler;
                samplers[secondSamplerIndex] = secondSampler;
            }
            
            //Update clip times
            {
                var loopDuration = firstSampler.Clip.duration * firstSampler.Weight +
                                   secondSampler.Clip.duration * secondSampler.Weight;

                var invLoopDuration = 1.0f / loopDuration;
                var stateSpeed = Speed;
                for (var i = startIndex; i <= endIndex; i++)
                {
                    var sampler = samplers[i];
                    var samplerSpeed = stateSpeed * sampler.Clip.duration * invLoopDuration;
                    sampler.PreviousNormalizedTime = sampler.NormalizedTime;
                    sampler.NormalizedTime += dt * samplerSpeed;

                    if (Loop)
                    {
                        sampler.LoopToClipTime();
                    }
                    samplers[i] = sampler;
                }
            }
        }
    }
}