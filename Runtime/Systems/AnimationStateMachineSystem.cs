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
        private EntityCommandBufferSystem ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            var updateFmsHandle = new UpdateStateMachineJob()
            {
                DeltaTime = Time.DeltaTime,
                Ecb = ecb
            }.ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
            
            //Sample bones (those only depend on updateFmsHandle)
            var sampleOptimizedHandle = new SampleOptimizedBonesJob()
            {
            }.ScheduleParallel(updateFmsHandle);
            var sampleNonOptimizedHandle = new SampleNonOptimizedBones()
            {
                CfeAnimationState = GetBufferFromEntity<AnimationState>(true),
                CfeClipSampler = GetBufferFromEntity<ClipSampler>(true),
                CfeStateMachine = GetComponentDataFromEntity<AnimationStateMachine>(true),
            }.ScheduleParallel(updateFmsHandle);
            
            var sampleRootHandle = new SampleRootJob()
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel(updateFmsHandle);

            var transferRootMotionHandle = new TransferRootMotionJob()
            {
                CfeDeltaPosition = GetComponentDataFromEntity<RootDeltaPosition>(true),
                CfeDeltaRotation = GetComponentDataFromEntity<RootDeltaRotation>(true),
            }.ScheduleParallel(sampleRootHandle);
            //end sample bones
            
            Dependency = JobHandle.CombineDependencies(sampleOptimizedHandle, sampleNonOptimizedHandle, transferRootMotionHandle);
        }
    }
}