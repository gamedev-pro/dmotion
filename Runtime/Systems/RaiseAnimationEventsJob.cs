using System.Runtime.CompilerServices;
using BovineLabs.Event.Containers;
using Unity.Burst;
using Unity.Entities;
using UnityEngine.Assertions;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct RaiseAnimationEventsJob : IJobEntity
    {
        public NativeEventStream.ThreadWriter Writer;
        public float DeltaTime;
        
        public void Execute(
            Entity animatorEntity,
            in AnimationStateMachine stateMachine,
            in DynamicBuffer<ClipSampler> samplers,
            in DynamicBuffer<AnimationState> states,
            in AnimatorEntity animatorOwner,
            in DynamicBuffer<AnimationEvent> animationEvents
        )
        {
            //Raise events
            RaiseStateEvents(animatorEntity, stateMachine.CurrentState, animatorOwner.Owner, states, samplers,
                animationEvents);
            if (stateMachine.NextState.IsValid)
            {
                RaiseStateEvents(animatorEntity, stateMachine.NextState, animatorOwner.Owner, states, samplers,
                    animationEvents);
            }
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RaiseStateEvents(Entity animatorEntity,
            in AnimationStateMachine.StateRef stateRef,
            Entity ownerEntity, in DynamicBuffer<AnimationState> states, in DynamicBuffer<ClipSampler> samplers,
            in DynamicBuffer<AnimationEvent> events)
        {
            Assert.IsTrue(states.IsValidIndex(stateRef.StateIndex));
            var state = states.ElementAtSafe(stateRef.StateIndex);
            var samplerIndex = state.GetActiveSamplerIndex(samplers);
            
            var sampler = samplers[samplerIndex];
            var normalizedSamplerTime = sampler.Clip.LoopToClipTime(sampler.Time);
            var previousSamplerTime = sampler.Clip.LoopToClipTime(sampler.Time - DeltaTime * sampler.Speed);
            
            for (var j = 0; j < events.Length; j++)
            {
                var e = events[j];
                if (e.SamplerIndex == samplerIndex && e.NormalizedTime >= previousSamplerTime &&
                    e.NormalizedTime <= normalizedSamplerTime)
                {
                    Writer.Write(new AnimationEventData()
                    {
                        EventHash = e.EventHash,
                        AnimatorEntity = animatorEntity,
                        AnimatorOwner = ownerEntity,
                    });
                }
            }
        }
    }
}