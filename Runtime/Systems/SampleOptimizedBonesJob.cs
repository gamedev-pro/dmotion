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
            //Sample blended (current and next states)
            if (stateMachine.NextState.IsValid)
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
                var nextState = states.ElementAtSafe(stateMachine.NextState.StateIndex);

                var blend = math.clamp(nextState.GetNormalizedStateTime(samplers) / nextState.TransitionDuration, 0, 1);
                
                var bones = boneToRootBuffer.AsBTRMatrixArray();
                //We start at 1 because root is sampled in a different job
                for (var i = 1; i < bones.Length; i++)
                {
                    var current = SingleClipSampling.SampleBoneBlended(i, blend, 0, currentState, nextState, samplers);
                    bones[i] = current.ToBTRMatrix(i, in bones, in hierarchyRef);
                }
            }
            //Sample current state
            else
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
                var bones = boneToRootBuffer.AsBTRMatrixArray();
                //We start at 1 because root is sampled in a different job
                for (var i = 1; i < bones.Length; i++)
                {
                    var current = currentState.SampleBone(i, 0, samplers);
                    bones[i] = current.ToBTRMatrix(i, in bones, in hierarchyRef);
                }
            }
        }
    }
}