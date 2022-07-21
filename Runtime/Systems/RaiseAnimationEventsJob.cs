using BovineLabs.Event.Containers;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct RaiseAnimationEventsJob : IJobEntity
    {
        internal void Execute(
            ref DynamicBuffer<RaisedAnimationEvent> raisedAnimationEvents,
            in DynamicBuffer<ClipSampler> samplers
        )
        {
            raisedAnimationEvents.Clear();
            if (TryGetHighestWeightSamplerIndex(samplers, out var samplerIndex))
            {
                var sampler = samplers[samplerIndex];
                var clipIndex = sampler.ClipIndex;
                var previousSamplerTime = sampler.PreviousNormalizedTime;
                var currentSamplerTime = sampler.NormalizedTime;
                ref var clipEvents = ref sampler.ClipEventsBlob.Value.ClipEvents[clipIndex].Events;
                for (short i = 0; i < clipEvents.Length; i++)
                {
                    ref var e = ref clipEvents[i];
                    if (e.ClipIndex == clipIndex &&
                        e.NormalizedTime >= previousSamplerTime && e.NormalizedTime <= currentSamplerTime)
                    {
                        raisedAnimationEvents.Add(new RaisedAnimationEvent()
                        {
                            EventHash = e.EventHash,
                        });
                    }
                }
            }
        }

        private bool TryGetHighestWeightSamplerIndex(in DynamicBuffer<ClipSampler> samplers, out byte samplerIndex)
        {
            var maxWeight = 0.0f;
            var maxWeightSamplerIndex = -1;
            for (byte i = 0; i < samplers.Length; i++)
            {
                if (samplers[i].Weight > maxWeight)
                {
                    maxWeight = samplers[i].Weight;
                    maxWeightSamplerIndex = i;
                }
            }

            if (maxWeightSamplerIndex > 0)
            {
                samplerIndex = (byte)maxWeightSamplerIndex;
                return true;
            }

            samplerIndex = 0;
            return false;
        }
    }
}