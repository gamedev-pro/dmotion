using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

namespace DOTSAnimation
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
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
            Dependency = new SyncBlendParametersJob()
            {
            }.ScheduleParallel();
            
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            Dependency = new RaiseAnimationEventsJob()
            {
                DeltaTime = Time.DeltaTime,
                Ecb = ecb
            }.ScheduleParallel();
            
            ecbSystem.AddJobHandleForProducer(Dependency);
            
            var sampleOptimizedBones = new SampleOptimizedBonesJob()
            {
            }.ScheduleParallel();
            var sampleNonOptimizedBones = new SampleNonOptimizedBones()
            {
                CfeAnimationState = GetBufferFromEntity<AnimationState>(),
                CfeClipSampler = GetBufferFromEntity<ClipSampler>(),
                CfeStateMachine = GetComponentDataFromEntity<AnimationStateMachine>(),
            }.ScheduleParallel();
            
            var sampleRootHandle = new SampleRootJob()
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel();
            
            var updateStateMachineHandle = new UpdateStateMachineJob()
            {
                DeltaTime = Time.DeltaTime,
            }.ScheduleParallel();
            
            var transferRootHandle = new TransferRootMotionJob()
            {
                CfeDeltaPosition = GetComponentDataFromEntity<RootDeltaPosition>(),
                CfeDeltaRotation = GetComponentDataFromEntity<RootDeltaRotation>(),
            }.ScheduleParallel(sampleRootHandle);
            
            Dependency = JobHandle.CombineDependencies(Dependency, sampleOptimizedBones, sampleNonOptimizedBones);
            Dependency = JobHandle.CombineDependencies(Dependency, updateStateMachineHandle, transferRootHandle);
        }
    }
}