using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

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
                var previousSamplerTime = sampler.PreviousTime;
                var currentSamplerTime = sampler.Time;
                ref var clipEvents = ref sampler.ClipEventsBlob.Value.ClipEvents[clipIndex].Events;
                for (short i = 0; i < clipEvents.Length; i++)
                {
                    ref var e = ref clipEvents[i];
                    bool shouldRaiseEvent;
                    
                    if (previousSamplerTime > currentSamplerTime)
                    {
                        //this mean we looped the clip
                        shouldRaiseEvent = (e.ClipTime > previousSamplerTime && e.ClipTime <= sampler.Clip.duration) ||
                                           (e.ClipTime > 0 && e.ClipTime <= currentSamplerTime);
                    }
                    else
                    {
                        shouldRaiseEvent = e.ClipTime > previousSamplerTime &&
                                           e.ClipTime <= currentSamplerTime;
                    }

                    if (shouldRaiseEvent)
                    {
                        var str = FixedString.Format("Raising even for clip {0}\np: {1}, c: {2} ({3})", sampler.Clip.name, sampler.PreviousTime, sampler.Time, e.ClipTime);
                        Debug.Log(str);
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