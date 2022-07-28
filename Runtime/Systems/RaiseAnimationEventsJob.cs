using Unity.Burst;
using Unity.Entities;

namespace DMotion
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
            for (var samplerIndex = 0; samplerIndex < samplers.Length; samplerIndex++)
            {
                var sampler = samplers[samplerIndex];
                if (mathex.iszero(sampler.Weight))
                {
                    continue;
                }
                
                var clipIndex = sampler.ClipIndex;
                var previousSamplerTime = sampler.PreviousNormalizedTime;
                var currentSamplerTime = sampler.NormalizedTime;
                ref var clipEvents = ref sampler.ClipEventsBlob.Value.ClipEvents[clipIndex].Events;
                for (short i = 0; i < clipEvents.Length; i++)
                {
                    ref var e = ref clipEvents[i];
                    bool shouldRaiseEvent;
                    
                    if (previousSamplerTime > currentSamplerTime)
                    {
                        //this mean we looped the clip
                        shouldRaiseEvent = e.NormalizedTime >= previousSamplerTime && e.NormalizedTime <= 1 ||
                                           e.NormalizedTime >= 0 && e.NormalizedTime <= currentSamplerTime;
                    }
                    else
                    {
                        shouldRaiseEvent = e.NormalizedTime >= previousSamplerTime &&
                                           e.NormalizedTime <= currentSamplerTime;
                    }

                    if (shouldRaiseEvent)
                    {
                        raisedAnimationEvents.Add(new RaisedAnimationEvent()
                        {
                            EventHash = e.EventHash,
                            ClipWeight = sampler.Weight,
                            ClipHandle = new SkeletonClipHandle(sampler.Clips, sampler.ClipIndex),
                        });
                    }
                }
            }
        }
    }
}