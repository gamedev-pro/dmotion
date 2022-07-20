using BovineLabs.Event.Containers;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct RaiseAnimationEventsJob : IJobEntity
    {
        internal NativeEventStream.ThreadWriter Writer;
        internal float DeltaTime;
        internal void Execute(
            Entity animatorEntity,
            in AnimationStateMachine stateMachine,
            in DynamicBuffer<ClipSampler> samplers,
            in ActiveSamplersCount activeSamplersCount,
            in AnimatorEntity animatorOwner
        )
        {
            //TODO: Events should not be tied to a state machine, but to a separate blob instead (otherwise one shots can't raise events)
            ref var clipEvents = ref stateMachine.StateMachineBlob.Value.ClipEvents;
            for (byte samplerIndex = 0; samplerIndex < activeSamplersCount.Value; samplerIndex++)
            {
                var sampler = samplers[samplerIndex];
                var clipIndex = sampler.ClipIndex;
                var previousSamplerTime = sampler.PreviousNormalizedTime;
                var currentSamplerTime = sampler.NormalizedTime;
                for (short i = 0; i < clipEvents.Length; i++)
                {
                    ref var e = ref clipEvents[i];
                    if (e.ClipIndex == clipIndex &&
                        e.NormalizedTime >= previousSamplerTime && e.NormalizedTime <= currentSamplerTime)
                    {
                        Debug.Log("YO");
                        Writer.Write(new RaisedAnimationEvent()
                        {
                            EventHash = e.EventHash,
                            AnimatorEntity = animatorEntity,
                            AnimatorOwner = animatorOwner.Owner
                        });
                    }
                }
            }
        }
    }
}