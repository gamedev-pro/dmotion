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
    internal partial class AnimationStateMachineSystem : SystemBase
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
            
            var sampleRootHandle = new SampleRootJob()
            {
            }.ScheduleParallel(updateFmsHandle);

            var transferRootMotionHandle = new TransferRootMotionJob()
            {
                CfeDeltaPosition = GetComponentDataFromEntity<RootDeltaTranslation>(true),
                CfeDeltaRotation = GetComponentDataFromEntity<RootDeltaRotation>(true),
            }.ScheduleParallel(sampleRootHandle);
            //end sample bones
            
            Dependency = JobHandle.CombineDependencies(sampleOptimizedHandle, sampleNonOptimizedHandle, transferRootMotionHandle);
        }
    }
}