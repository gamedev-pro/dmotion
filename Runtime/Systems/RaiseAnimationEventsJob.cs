using BovineLabs.Event.Containers;
using Unity.Burst;
using Unity.Entities;

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

            if (TryGetHighestWeightSamplerIndex(samplers, activeSamplersCount, out var samplerIndex))
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

        private bool TryGetHighestWeightSamplerIndex(in DynamicBuffer<ClipSampler> samplers,
            in ActiveSamplersCount activeSamplersCount, out byte samplerIndex)
        {
            var maxWeight = 0.0f;
            var maxWeightSamplerIndex = -1;
            for (byte i = 0; i < activeSamplersCount.Value; i++)
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