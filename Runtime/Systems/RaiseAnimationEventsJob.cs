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
            stateMachine.CurrentState.RaiseStateEvents(DeltaTime, animatorEntity, animatorOwner.Owner, ref Writer);
            if (stateMachine.CurrentTransition.IsValid)
            {
                stateMachine.NextState.RaiseStateEvents(DeltaTime, animatorEntity, animatorOwner.Owner, ref Writer);
            }
        }
    }
}