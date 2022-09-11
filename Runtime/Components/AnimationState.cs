using System;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DMotion
{
    [BurstCompile]
    internal struct AnimationState
    {
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal short StateIndex;
        internal float Time;
        internal byte StartSamplerId;

        internal bool IsValid => StateIndex >= 0;
        internal static AnimationState Null => new AnimationState() { StateIndex = -1 };
        internal readonly ref AnimationStateBlob StateBlob => ref StateMachineBlob.Value.States[StateIndex];
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
            if (samplers.TryFindIdAndInsertIndex((byte)linearBlendState.ClipSortedByThreshold.Length, out var id,
                    out var insertIndex))
            {
                samplers.Length += linearBlendState.ClipSortedByThreshold.Length;
                StartSamplerId = id;
                for (var i = 0; i < linearBlendState.ClipSortedByThreshold.Length; i++)
                {
                    var clip = linearBlendState.ClipSortedByThreshold[i];

                    var index = i + insertIndex;
                    samplers[index] = new ClipSampler
                    {
                        Id = (byte)(StartSamplerId + i),
                        ClipIndex = clip.ClipIndex,
                        Clips = clips,
                        ClipEventsBlob = clipEvents,
                        PreviousTime = 0,
                        Time = 0,
                        Weight = 0
                    };
                }
            }
        }

        private void Initialize_Single(
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var singleClip = ref AsSingleClip;
            StartSamplerId = samplers.AddWithId(new ClipSampler
            {
                ClipIndex = singleClip.ClipIndex,
                Clips = clips,
                ClipEventsBlob = clipEvents,
                PreviousTime = 0,
                Time = 0,
                Weight = 0
            });
        }

        internal void UpdateSamplers(float dt, float blendWeight, in DynamicBuffer<BlendParameter> blendParameters,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            Time += dt * Speed;
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
            var samplerIndex = samplers.IdToIndex(StartSamplerId);
            var sampler = samplers[samplerIndex];
            sampler.Weight = blendWeight;

            sampler.PreviousTime = sampler.Time;
            sampler.Time += dt * Speed;
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
            ref var linearBlendState = ref AsLinearBlend;
            ref var sortedClips = ref linearBlendState.ClipSortedByThreshold;
            var startIndex = samplers.IdToIndex(StartSamplerId);
            var endIndex = startIndex + sortedClips.Length - 1;

            var blendRatio = blendParameters[linearBlendState.BlendParameterIndex].Value;

            //we assume thresholds are sorted here
            blendRatio = math.clamp(blendRatio, sortedClips[0].Threshold, sortedClips[^1].Threshold);

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
                firstSampler.Weight = (1 - t) * blendWeight;
                secondSampler.Weight = t * blendWeight;
                samplers[firstSamplerIndex] = firstSampler;
                samplers[secondSamplerIndex] = secondSampler;
            }

            //Update clip times
            {
                var loopDuration = firstSampler.Clip.duration * firstSampler.Weight +
                                   secondSampler.Clip.duration * secondSampler.Weight;

                //We don't want to divide by zero if our weight is zero
                if (loopDuration > 0)
                {
                    var invLoopDuration = 1.0f / loopDuration;
                    var stateSpeed = Speed;
                    for (var i = startIndex; i <= endIndex; i++)
                    {
                        var sampler = samplers[i];
                        var samplerSpeed = stateSpeed * sampler.Clip.duration * invLoopDuration;
                        sampler.PreviousTime = sampler.Time;
                        sampler.Time += dt * samplerSpeed;

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
}