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
            new("SampleOptimizedBonesJob");

        internal static readonly ProfilerMarker Marker_SampleNonOptimizedBonesJob =
            new("SampleNonOptimizedBonesJob");

        internal static readonly ProfilerMarker Marker_SampleRootDeltasJob =
            new("SampleRootDeltasJob");

        internal static readonly ProfilerMarker Marker_ApplyRootMotionToEntityJob =
            new("ApplyRootMotionToEntityJob");

        internal static readonly ProfilerMarker Marker_TransferRootMotionJob =
            new("TransferRootMotionJob");

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
                BfeClipSampler = SystemAPI.GetBufferLookup<ClipSampler>(true),
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
                CfeDeltaPosition = SystemAPI.GetComponentLookup<RootDeltaTranslation>(true),
                CfeDeltaRotation = SystemAPI.GetComponentLookup<RootDeltaRotation>(true),
                Marker = Marker_TransferRootMotionJob
            }.ScheduleParallel(sampleRootDeltasHandle);
            //end sample bones

            Dependency = JobHandle.CombineDependencies(sampleOptimizedHandle, sampleNonOptimizedHandle,
                transferRootMotionHandle);
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
                BfeClipSampler = SystemAPI.GetBufferLookup<ClipSampler>(true),
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
                CfeDeltaPosition = SystemAPI.GetComponentLookup<RootDeltaTranslation>(true),
                CfeDeltaRotation = SystemAPI.GetComponentLookup<RootDeltaRotation>(true),
                Marker = Marker_TransferRootMotionJob
            }.ScheduleParallel();
        }
    }
}