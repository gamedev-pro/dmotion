using System.Collections.Generic;
using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    internal static class ClipSamplerTestUtils
    {
        internal static ClipSampler GetFirstSamplerForAnimationState(EntityManager manager, Entity entity, byte animationStateId)
        {
            var animationState = AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, animationStateId);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            var sampler = clipSamplers.GetWithId(animationState.StartSamplerId);
            return sampler;
        }

        internal static int AnimationStateStartSamplerIdToIndex(EntityManager manager, Entity entity, byte animationStateId)
        {
            var animationState = AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, animationStateId);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            return clipSamplers.IdToIndex(animationState.StartSamplerId);
        }

        internal static IEnumerable<ClipSampler> GetAllSamplersForAnimationState(EntityManager manager, Entity entity,
            byte animationStateId)
        {
            var animationState = AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, animationStateId);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            var startIndex = clipSamplers.IdToIndex(animationState.StartSamplerId);
            for (var i = startIndex; i < startIndex + animationState.ClipCount; i++)
            {
                yield return clipSamplers[i];
            }
        }
        
        internal static void AssertSamplerUpdate(ECSTestBase ecsTest, Entity entity, int[] samplerIndexes)
        {
        }
    }
}