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
        internal float DeltaTime;
        internal void Execute(
            ref RootDeltaPosition rootDeltaPosition,
            ref RootDeltaRotation rootDeltaRotation,
            in AnimationStateMachine stateMachine
        )
        {
            //Sample root blended and calculate motion deltas
            if (stateMachine.CurrentTransition.IsValid)
            {
                var blend = math.clamp(
                    stateMachine.NextState.NormalizedTime/ stateMachine.CurrentTransitionBlob.NormalizedTransitionDuration, 0, 1);

                var prevCurrentStateTime = stateMachine.CurrentState.GetNormalizedTimeShifted(DeltaTime);
                var prevNextStateTime = stateMachine.NextState.GetNormalizedTimeShifted(DeltaTime);
                
                var prevRoot = SingleClipSampling.SampleBoneBlended(stateMachine.CurrentState,
                    prevCurrentStateTime,
                    stateMachine.NextState, prevNextStateTime,
                    blend, 0);
                
                var newRoot = SingleClipSampling.SampleBoneBlended(stateMachine.CurrentState,
                    stateMachine.CurrentState.NormalizedTime,
                    stateMachine.NextState, stateMachine.NextState.NormalizedTime,
                    blend, 0);
                
                rootDeltaPosition.Value = newRoot.translation - prevRoot.translation;
                rootDeltaRotation.Value = mathex.delta(prevRoot.rotation, newRoot.rotation);
            }
            //Sample root and calculate motion deltas
            else
            {
                var prevCurrentStateTime =
                    stateMachine.CurrentState.NormalizedTime - DeltaTime*stateMachine.CurrentState.Speed;

                var prevRoot = stateMachine.CurrentState.SampleBone(prevCurrentStateTime, 0);
                var newRoot = stateMachine.CurrentState.SampleBone(stateMachine.CurrentState.NormalizedTime, 0);
                
                rootDeltaPosition.Value = newRoot.translation - prevRoot.translation;
                rootDeltaRotation.Value = mathex.delta(prevRoot.rotation, newRoot.rotation);
            }
        }
    }
}