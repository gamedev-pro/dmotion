using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using UnityEngine.Assertions;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct RaiseAnimationEventsJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public float DeltaTime;
        
        public void Execute(
            Entity e,
            [EntityInQueryIndex] int sortKey,
            in AnimationStateMachine stateMachine,
            in DynamicBuffer<ClipSampler> samplers,
            in DynamicBuffer<AnimationState> states,
            in AnimatorEntity animatorOwner,
            in DynamicBuffer<AnimationEvent> animationEvents
        )
        {
            //Raise events
            RaiseStateEvents(sortKey, stateMachine.CurrentState, animatorOwner.Owner, states, samplers,
                animationEvents);
            if (stateMachine.NextState.IsValid)
            {
                RaiseStateEvents(sortKey, stateMachine.NextState, animatorOwner.Owner, states, samplers,
                    animationEvents);
            }
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RaiseStateEvents(
            int sortKey, in AnimationStateMachine.StateRef stateRef,
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
                unsafe
                {
                    if (e.SamplerIndex == samplerIndex && e.NormalizedTime >= previousSamplerTime &&
                        e.NormalizedTime <= normalizedSamplerTime)
                    {
                        var ecb = Ecb;
                        e.Delegate.Invoke(&ownerEntity, sortKey, &ecb);
                    }
                }
            }
        }
    }
}