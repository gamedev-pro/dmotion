using System.Runtime.CompilerServices;
using Unity.Burst;
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
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct ClipSamplingSystem : ISystem
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

        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
#if DEBUG || UNITY_EDITOR
            if (JobsUtility.JobDebuggerEnabled)
            {
                OnUpdate_Safe(ref state);
            }
            else
            {
                OnUpdate_Unsafe(ref state);
            }
#else
            OnUpdate_Unsafe(ref state);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUpdate_Unsafe(ref SystemState state)
        {
            // new NormalizedSamplersWeights().ScheduleParallel();
            //Sample bones (those only depend on updateFmsHandle)
            var sampleOptimizedHandle = new SampleOptimizedBonesJob
            {
                Marker = Marker_SampleOptimizedBonesJob
            }.ScheduleParallel(state.Dependency);

            var sampleNonOptimizedHandle = new SampleNonOptimizedBones
            {
                BfeClipSampler = SystemAPI.GetBufferLookup<ClipSampler>(true),
                Marker = Marker_SampleNonOptimizedBonesJob
            }.ScheduleParallel(state.Dependency);

            var sampleRootDeltasHandle = new SampleRootDeltasJob
            {
                Marker = Marker_SampleRootDeltasJob
            }.ScheduleParallel(state.Dependency);

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

            state.Dependency = JobHandle.CombineDependencies(sampleOptimizedHandle, sampleNonOptimizedHandle,
                transferRootMotionHandle);
            state.Dependency = JobHandle.CombineDependencies(state.Dependency, applyRootMotionHandle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUpdate_Safe(ref SystemState state)
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