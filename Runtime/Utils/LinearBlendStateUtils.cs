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
            Assert.AreEqual(linearBlendBlob.SortedClipIndexes.Length, linearBlendBlob.SortedClipThresholds.Length);
            var clipCount = (byte)linearBlendBlob.SortedClipIndexes.Length;

            var newSamplers =
                new NativeArray<ClipSampler>(clipCount, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            for (var i = 0; i < clipCount; i++)
            {
                var clipIndex = (ushort) linearBlendBlob.SortedClipIndexes[i];
                newSamplers[i] = new ClipSampler
                {
                    ClipIndex = clipIndex,
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
            in NativeArray<float> thresholds,
            in PlayableState playable,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            Assert.IsTrue(thresholds.IsCreated);
            var startIndex = samplers.IdToIndex(playable.StartSamplerId);
            var endIndex = startIndex + thresholds.Length - 1;

            //we assume thresholds are sorted here
            blendRatio = math.clamp(blendRatio, thresholds[0], thresholds[^1]);

            //find clip tuple to be blended
            var firstClipIndex = -1;
            var secondClipIndex = -1;
            for (var i = 1; i < thresholds.Length; i++)
            {
                var currentThreshold = thresholds[i];
                var prevThreshold = thresholds[i - 1];
                if (blendRatio >= prevThreshold && blendRatio <= currentThreshold)
                {
                    firstClipIndex = i - 1;
                    secondClipIndex = i;
                    break;
                }
            }

            Assert.IsTrue(firstClipIndex >= 0 && secondClipIndex >= 0);

            var firstThreshold = thresholds[firstClipIndex];
            var secondThreshold = thresholds[secondClipIndex];

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

                var t = (blendRatio - firstThreshold) / (secondThreshold - firstThreshold);
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