using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace DMotion
{
    [BurstCompile]
    internal struct LinearBlendAnimationStateMachineState : IBufferElementData
    {
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal short StateIndex;
        internal byte PlayableId;
        internal readonly ref AnimationStateBlob StateBlob => ref StateMachineBlob.Value.States[StateIndex];
        internal readonly StateType Type => StateBlob.Type;

        internal readonly ref LinearBlendStateBlob AsLinearBlend =>
            ref StateMachineBlob.Value.LinearBlendStates[StateBlob.StateIndex];

        internal static LinearBlendAnimationStateMachineState New(
            short stateIndex,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<LinearBlendAnimationStateMachineState> linearBlendStates,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var linearBlendState = new LinearBlendAnimationStateMachineState
            {
                StateMachineBlob = stateMachineBlob,
                StateIndex = stateIndex
            };

            ref var linearBlendBlob = ref linearBlendState.AsLinearBlend;
            var clipCount = (byte)linearBlendBlob.ClipSortedByThreshold.Length;

            var newSamplers =
                new NativeArray<ClipSampler>(clipCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < clipCount; i++)
            {
                var clip = linearBlendBlob.ClipSortedByThreshold[i];
                newSamplers[i] = new ClipSampler
                {
                    ClipIndex = clip.ClipIndex,
                    Clips = clips,
                    ClipEventsBlob = clipEvents,
                    PreviousTime = 0,
                    Time = 0,
                    Weight = 0
                };
            }

            var playableIndex = PlayableState.New(ref playableStates, ref samplers, newSamplers, linearBlendState.StateBlob.Speed,
                linearBlendState.StateBlob.Loop);

            linearBlendState.PlayableId = playableStates[playableIndex].Id;
            linearBlendStates.Add(linearBlendState);

            return linearBlendState;
        }

        internal void UpdateSamplers(
            float dt,
            in PlayableState playable,
            in DynamicBuffer<BlendParameter> blendParameters,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var linearBlendState = ref AsLinearBlend;
            ref var sortedClips = ref linearBlendState.ClipSortedByThreshold;
            var startIndex = samplers.IdToIndex(playable.StartSamplerId);
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
                firstSampler.Weight = (1 - t) * playable.Weight;
                secondSampler.Weight = t * playable.Weight;
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
                    var stateSpeed = playable.Speed;
                    for (var i = startIndex; i <= endIndex; i++)
                    {
                        var sampler = samplers[i];
                        var samplerSpeed = stateSpeed * sampler.Clip.duration * invLoopDuration;
                        sampler.PreviousTime = sampler.Time;
                        sampler.Time += dt * samplerSpeed;

                        if (playable.Loop)
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