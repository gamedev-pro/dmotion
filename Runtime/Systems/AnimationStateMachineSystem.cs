using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(TRSToLocalToParentSystem))]
    [UpdateBefore(typeof(TRSToLocalToWorldSystem))]
    public partial class AnimationStateMachineSystem : SystemBase
    {
        internal static readonly ProfilerMarker Marker_SampleOptimizedBonesJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(SampleOptimizedBonesJob));
        
        internal static readonly ProfilerMarker Marker_SampleNonOptimizedBonesJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(Marker_SampleNonOptimizedBonesJob));
        
        internal static readonly ProfilerMarker Marker_UpdateStateMachineJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(UpdateStateMachineJob));
        
        internal static readonly ProfilerMarker Marker_SampleRootDeltasJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(SampleRootDeltasJob));
        
        internal static readonly ProfilerMarker Marker_ApplyRootMotionToEntityJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(ApplyRootMotionToEntityJob));
        
        internal static readonly ProfilerMarker Marker_TransferRootMotionJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(TransferRootMotionJob));
        
        protected override void OnUpdate()
        {
            var updateFmsHandle = new UpdateStateMachineJob
            {
                DeltaTime = Time.DeltaTime,
                Marker = Marker_UpdateStateMachineJob
            }.ScheduleParallel();
            
            //Sample bones (those only depend on updateFmsHandle)
            var sampleOptimizedHandle = new SampleOptimizedBonesJob
            {
                Marker = Marker_SampleOptimizedBonesJob
            }.ScheduleParallel(updateFmsHandle);
            
            var sampleNonOptimizedHandle = new SampleNonOptimizedBones
            {
                BfeClipSampler = GetBufferFromEntity<ClipSampler>(true),
                Marker = Marker_SampleNonOptimizedBonesJob
            }.ScheduleParallel(updateFmsHandle);
            
            var sampleRootDeltasHandle = new SampleRootDeltasJob
            {
                Marker = Marker_SampleRootDeltasJob
            }.ScheduleParallel(updateFmsHandle);
            
            var applyRootMotionHandle = new ApplyRootMotionToEntityJob
            {
                Marker = Marker_ApplyRootMotionToEntityJob
            }.ScheduleParallel(sampleRootDeltasHandle);
            
            var transferRootMotionHandle = new TransferRootMotionJob
            {
                CfeDeltaPosition = GetComponentDataFromEntity<RootDeltaTranslation>(true),
                CfeDeltaRotation = GetComponentDataFromEntity<RootDeltaRotation>(true),
                Marker = Marker_TransferRootMotionJob
            }.ScheduleParallel(sampleRootDeltasHandle);
            //end sample bones
            
            Dependency = JobHandle.CombineDependencies(sampleOptimizedHandle, sampleNonOptimizedHandle, transferRootMotionHandle);
            Dependency = JobHandle.CombineDependencies(Dependency, applyRootMotionHandle);
        }
    }
}