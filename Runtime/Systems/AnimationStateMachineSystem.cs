using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTSAnimation
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(TRSToLocalToParentSystem))]
    [UpdateBefore(typeof(TRSToLocalToWorldSystem))]
    public partial class AnimationStateMachineSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var updateFmsHandle = new UpdateStateMachineJob()
            {
                DeltaTime = Time.DeltaTime,
            }.ScheduleParallel();
            
            //Sample bones (those only depend on updateFmsHandle)
            var sampleOptimizedHandle = new SampleOptimizedBonesJob()
            {
            }.ScheduleParallel(updateFmsHandle);
            var sampleNonOptimizedHandle = new SampleNonOptimizedBones()
            {
                BfeClipSampler = GetBufferFromEntity<ClipSampler>(true),
            }.ScheduleParallel(updateFmsHandle);
            
            var sampleRootDeltasHandle = new SampleRootDeltasJob()
            {
            }.ScheduleParallel(updateFmsHandle);
            
            var applyRootMotionHandle = new ApplyRootMotionToEntityJob()
            {
            }.ScheduleParallel(sampleRootDeltasHandle);
            
            var transferRootMotionHandle = new TransferRootMotionJob()
            {
                CfeDeltaPosition = GetComponentDataFromEntity<RootDeltaTranslation>(true),
                CfeDeltaRotation = GetComponentDataFromEntity<RootDeltaRotation>(true),
            }.ScheduleParallel(sampleRootDeltasHandle);
            //end sample bones
            
            Dependency = JobHandle.CombineDependencies(sampleOptimizedHandle, sampleNonOptimizedHandle, transferRootMotionHandle);
            Dependency = JobHandle.CombineDependencies(Dependency, applyRootMotionHandle);
        }
    }
}