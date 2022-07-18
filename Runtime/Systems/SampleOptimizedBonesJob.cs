using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct SampleOptimizedBonesJob : IJobEntity
    {
        internal void Execute(
            ref DynamicBuffer<OptimizedBoneToRoot> boneToRootBuffer,
            in AnimationStateMachine stateMachine,
            in OptimizedSkeletonHierarchyBlobReference hierarchyRef)
        {
            var blender = new BufferPoseBlender(boneToRootBuffer);
            var requiresNormalization = true;
            
            //Sample blended (current and next states)
            if (stateMachine.NextState.IsValid)
            {
                var blend = math.clamp(stateMachine.NextState.NormalizedTime /
                                       stateMachine.CurrentTransitionBlob.NormalizedTransitionDuration, 0, 1);
                stateMachine.CurrentState.SamplePose(stateMachine.CurrentState.NormalizedTime, 1-blend, ref blender);
                stateMachine.NextState.SamplePose(stateMachine.NextState.NormalizedTime, blend, ref blender);
            }
            //Sample current state
            else
            {
                //Skip normalization when there is only one pose since the rotations will already be normalized
                requiresNormalization = stateMachine.CurrentState.Type != StateType.Single;
                stateMachine.CurrentState.SamplePose(stateMachine.CurrentState.NormalizedTime, 1, ref blender);
            }
            
            if (requiresNormalization)
            {
                blender.NormalizeRotations();
            }
            blender.ApplyBoneHierarchyAndFinish(hierarchyRef.blob);
        }
    }
}