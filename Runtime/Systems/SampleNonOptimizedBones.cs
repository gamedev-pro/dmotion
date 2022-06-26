using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTSAnimation
{
    [BurstCompile]
    [WithNone(typeof(SkeletonRootTag))]
    internal partial struct SampleNonOptimizedBones : IJobEntity
    {
        [ReadOnly] public ComponentDataFromEntity<AnimationStateMachine> CfeStateMachine;
        [ReadOnly] public BufferFromEntity<ClipSampler> CfeClipSampler;
        [ReadOnly] public BufferFromEntity<AnimationState> CfeAnimationState;
        public void Execute(
            ref Translation translation,
            ref Rotation rotation,
            ref NonUniformScale scale,
            in BoneOwningSkeletonReference skeletonRef,
            in BoneIndex boneIndex
        )
        {
            var stateMachine = CfeStateMachine[skeletonRef.skeletonRoot];
            var states = CfeAnimationState[skeletonRef.skeletonRoot];
            var samplers = CfeClipSampler[skeletonRef.skeletonRoot];
            
            //Sample blended (current and next states)
            if (stateMachine.NextState.IsValid)
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
                var nextState = states.ElementAtSafe(stateMachine.NextState.StateIndex);

                var blend = math.clamp(nextState.GetNormalizedStateTime(samplers) / nextState.TransitionDuration, 0, 1);
                var bone = SingleClipSampling.SampleBoneBlended(boneIndex.index, blend, 0, currentState, nextState, samplers);
                translation.Value = bone.translation;
                rotation.Value = bone.rotation;
                scale.Value = bone.scale;
            }
            //Sample current state
            else
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
                var bone = currentState.SampleBone(boneIndex.index, timeShift:0 ,samplers);
                translation.Value = bone.translation;
                rotation.Value = bone.rotation;
                scale.Value = bone.scale;
            }
        }
    }
}