using System.Collections.Generic;
using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    internal static class ClipSamplerTestUtils
    {
        internal static ClipSampler GetFirstSamplerForPlayable(EntityManager manager, Entity entity, byte playableId)
        {
            var playable = PlayableTestUtils.GetPlayableFromEntity(manager, entity, playableId);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            var sampler = clipSamplers.GetWithId(playable.StartSamplerId);
            return sampler;
        }

        internal static int PlayableStartSamplerIdToIndex(EntityManager manager, Entity entity, byte playableId)
        {
            var playable = PlayableTestUtils.GetPlayableFromEntity(manager, entity, playableId);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            return clipSamplers.IdToIndex(playable.StartSamplerId);
        }

        internal static IEnumerable<ClipSampler> GetAllSamplersForPlayable(EntityManager manager, Entity entity,
            byte playableId)
        {
            var playable = PlayableTestUtils.GetPlayableFromEntity(manager, entity, playableId);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            var startIndex = clipSamplers.IdToIndex(playable.StartSamplerId);
            for (var i = startIndex; i < startIndex + playable.ClipCount; i++)
            {
                yield return clipSamplers[i];
            }
        }
        
        internal static void AssertSamplerUpdate(ECSTestsFixture ecsTest, Entity entity, int[] samplerIndexes)
        {
        }
    }
}