using Latios.Kinemation;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.Assertions;

namespace DMotion
{
    internal static class LinearBlendStateUtils
    {
        internal static LinearBlendAnimationStateMachineState NewForStateMachine(
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

            var playableIndex = PlayableState.New(ref playableStates, ref samplers, newSamplers,
                linearBlendState.StateBlob.Speed,
                linearBlendState.StateBlob.Loop);

            linearBlendState.PlayableId = playableStates[playableIndex].Id;
            linearBlendStates.Add(linearBlendState);

            return linearBlendState;
        }

        internal static void UpdateSamplers(
            float dt,
            float blendRatio,
            NativeArray<ClipWithThreshold> clipsAndThresholds,
            in PlayableState playable,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            Assert.IsTrue(clipsAndThresholds.IsCreated);
            var startIndex = samplers.IdToIndex(playable.StartSamplerId);
            var endIndex = startIndex + clipsAndThresholds.Length - 1;

            //we assume thresholds are sorted here
            blendRatio = math.clamp(blendRatio, clipsAndThresholds[0].Threshold, clipsAndThresholds[^1].Threshold);

            //find clip tuple to be blended
            var firstClipIndex = -1;
            var secondClipIndex = -1;
            for (var i = 1; i < clipsAndThresholds.Length; i++)
            {
                var currentThreshold = clipsAndThresholds[i].Threshold;
                var prevThreshold = clipsAndThresholds[i - 1].Threshold;
                if (blendRatio >= prevThreshold && blendRatio <= currentThreshold)
                {
                    firstClipIndex = i - 1;
                    secondClipIndex = i;
                    break;
                }
            }

            var firstThreshold = clipsAndThresholds[firstClipIndex];
            var secondThreshold = clipsAndThresholds[secondClipIndex];

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

                var t = (blendRatio - firstThreshold.Threshold) / (secondThreshold.Threshold - firstThreshold.Threshold);
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
                if (!mathex.iszero(loopDuration))
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