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
        [ReadOnly] internal ComponentDataFromEntity<AnimationStateMachine> CfeStateMachine;
        internal void Execute(
            ref Translation translation,
            ref Rotation rotation,
            ref NonUniformScale scale,
            in BoneOwningSkeletonReference skeletonRef,
            in BoneIndex boneIndex
        )
        {
            var stateMachine = CfeStateMachine[skeletonRef.skeletonRoot];
            
            //Sample blended (current and next states)
            if (stateMachine.CurrentTransition.IsValid)
            {
                var blend = math.clamp(
                    stateMachine.NextState.NormalizedTime / stateMachine.CurrentTransitionBlob.NormalizedTransitionDuration, 0, 1);

                var bone = SingleClipSampling.SampleBoneBlended(stateMachine.CurrentState,
                    stateMachine.CurrentState.NormalizedTime,
                    stateMachine.NextState, stateMachine.NextState.NormalizedTime,
                    blend, boneIndex.index);
                
                translation.Value = bone.translation;
                rotation.Value = bone.rotation;
                scale.Value = bone.scale;
            }
            //Sample current state
            else
            {
                var bone = stateMachine.CurrentState.SampleBone(stateMachine.CurrentState.NormalizedTime,
                    boneIndex.index);
                
                translation.Value = bone.translation;
                rotation.Value = bone.rotation;
                scale.Value = bone.scale;
            }
        }
    }
}