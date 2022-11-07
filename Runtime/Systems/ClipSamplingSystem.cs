using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(TRSToLocalToParentSystem))]
    [UpdateBefore(typeof(TRSToLocalToWorldSystem))]
    public partial class ClipSamplingSystem : SystemBase
    {
        internal static readonly ProfilerMarker Marker_SampleOptimizedBonesJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(SampleOptimizedBonesJob));
        
        internal static readonly ProfilerMarker Marker_SampleNonOptimizedBonesJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(Marker_SampleNonOptimizedBonesJob));
        
        internal static readonly ProfilerMarker Marker_SampleRootDeltasJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(SampleRootDeltasJob));
        
        internal static readonly ProfilerMarker Marker_ApplyRootMotionToEntityJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(ApplyRootMotionToEntityJob));
        
        internal static readonly ProfilerMarker Marker_TransferRootMotionJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(TransferRootMotionJob));
        
        protected override void OnUpdate()
        {
#if DEBUG || UNITY_EDITOR
            if (JobsUtility.JobDebuggerEnabled)
            {
                OnUpdate_Safe();
            }
            else
            {
                OnUpdate_Unsafe();
            }
#else
            OnUpdate_Unsafe();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUpdate_Unsafe()
        {
            // new NormalizedSamplersWeights().ScheduleParallel();
            //Sample bones (those only depend on updateFmsHandle)
            var sampleOptimizedHandle = new SampleOptimizedBonesJob
            {
                Marker = Marker_SampleOptimizedBonesJob
            }.ScheduleParallel(Dependency);
            
            var sampleNonOptimizedHandle = new SampleNonOptimizedBones
            {
                BfeClipSampler = GetBufferFromEntity<ClipSampler>(true),
                Marker = Marker_SampleNonOptimizedBonesJob
            }.ScheduleParallel(Dependency);
            
            var sampleRootDeltasHandle = new SampleRootDeltasJob
            {
                Marker = Marker_SampleRootDeltasJob
            }.ScheduleParallel(Dependency);
            
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
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUpdate_Safe()
        {
            new SampleOptimizedBonesJob
            {
                Marker = Marker_SampleOptimizedBonesJob
            }.ScheduleParallel();
            
            new SampleNonOptimizedBones
            {
                BfeClipSampler = GetBufferFromEntity<ClipSampler>(true),
                Marker = Marker_SampleNonOptimizedBonesJob
            }.ScheduleParallel();
            
            new SampleRootDeltasJob
            {
                Marker = Marker_SampleRootDeltasJob
            }.ScheduleParallel();
            
            new ApplyRootMotionToEntityJob
            {
                Marker = Marker_ApplyRootMotionToEntityJob
            }.ScheduleParallel();
            
            new TransferRootMotionJob
            {
                CfeDeltaPosition = GetComponentDataFromEntity<RootDeltaTranslation>(true),
                CfeDeltaRotation = GetComponentDataFromEntity<RootDeltaRotation>(true),
                Marker = Marker_TransferRootMotionJob
            }.ScheduleParallel();
        }
    }
}