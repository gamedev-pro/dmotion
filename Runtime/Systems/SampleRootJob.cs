using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    [WithAll(typeof(SkeletonRootTag))]
    internal partial struct SampleRootJob : IJobEntity
    {
        public float DeltaTime;
        public void Execute(
            ref RootDeltaPosition rootDeltaPosition,
            ref RootDeltaRotation rootDeltaRotation,
            in AnimationStateMachine stateMachine,
            in DynamicBuffer<ClipSampler> samplers,
            in DynamicBuffer<AnimationState> states
        )
        {
            //Sample blended (current and next states)
            if (stateMachine.NextState.IsValid)
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
                var nextState = states.ElementAtSafe(stateMachine.NextState.StateIndex);

                var blend = math.clamp(nextState.GetNormalizedStateTime(samplers) / nextState.TransitionDuration, 0, 1);

                //Calculate root motion deltas
                var prevRoot = SingleClipSampling.SampleBoneBlended(0, blend, -DeltaTime, currentState, nextState, samplers);
                var newRoot = SingleClipSampling.SampleBoneBlended(0, blend, 0, currentState, nextState, samplers);
                rootDeltaPosition.Value = newRoot.translation - prevRoot.translation;
                rootDeltaRotation.Value = mathex.delta(prevRoot.rotation, newRoot.rotation);
            }
            //Sample current state
            else
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);

                //Calculate root motion deltas
                var prevRoot = currentState.SampleBone(0, -DeltaTime, samplers);
                var newRoot = currentState.SampleBone(0, timeShift:0 ,samplers);
                rootDeltaPosition.Value = newRoot.translation - prevRoot.translation;
                rootDeltaRotation.Value = mathex.delta(prevRoot.rotation, newRoot.rotation);
            }
        }
    }
}