using Latios.Kinemation;
using Unity.Entities;

namespace DMotion.Tests
{
    internal static class ClipSamplerTestUtils
    {
        internal static ClipSampler GetSamplerForPlayable(EntityManager manager, Entity entity, byte playableId)
        {
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            var sampler = clipSamplers.GetWithId(playableId);
            return sampler;
        }
    }
}