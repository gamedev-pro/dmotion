using System.Runtime.CompilerServices;
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
            in AnimatorEntity animatorOwner
        )
        {
            //Raise events
            RaiseStateEvents(stateMachine, stateMachine.CurrentState, animatorEntity, animatorOwner.Owner);
            if (stateMachine.CurrentTransition.IsValid)
            {
                RaiseStateEvents(stateMachine, stateMachine.CurrentState, animatorEntity, animatorOwner.Owner);
            }
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RaiseStateEvents(in AnimationStateMachine stateMachine, in AnimationState state,
            Entity animatorEntity, Entity ownerEntity)
        {
            var currentNormalizedTime = state.NormalizedTime;
            var previousNormalizedTime = state.NormalizedTime - DeltaTime * state.Speed;
            for (var i = 0; i < stateMachine.EventsBlob.Length; i++)
            {
                var e = stateMachine.EventsBlob[i];
                if (e.StateIndex == state.StateIndex && e.NormalizedTime >= previousNormalizedTime &&
                    e.NormalizedTime <= currentNormalizedTime)
                {
                    Writer.Write(new RaisedAnimationEvent()
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