using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct SampleOptimizedBonesJob : IJobEntity
    {
        public void Execute(
            ref DynamicBuffer<OptimizedBoneToRoot> boneToRootBuffer,
            in AnimationStateMachine stateMachine,
            in DynamicBuffer<ClipSampler> samplers,
            in DynamicBuffer<AnimationState> states,
            in OptimizedSkeletonHierarchyBlobReference hierarchyRef)
        {
            var blender = new BufferPoseBlender(boneToRootBuffer);
            bool requiresNormalization = true;
            
            //Sample blended (current and next states)
            if (stateMachine.NextState.IsValid)
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
                var nextState = states.ElementAtSafe(stateMachine.NextState.StateIndex);
            
                var blend = math.clamp(nextState.GetNormalizedStateTime(samplers) / nextState.TransitionDuration, 0, 1);
            
                SingleClipSampling.SamplePoseBlended(ref blender, blend, 0, currentState, nextState, samplers);
            }
            //Sample current state
            else
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
            
                //Skip normalization when there is only one pose since the rotations will already be normalized
                requiresNormalization = currentState.Type != AnimationSamplerType.Single;
            
                currentState.SamplePose(ref blender, 0, samplers);
            }
            
            if (requiresNormalization)
            {
                blender.NormalizeRotations();
            }
            
            blender.ApplyBoneHierarchyAndFinish(hierarchyRef.blob);
        }
    }
}