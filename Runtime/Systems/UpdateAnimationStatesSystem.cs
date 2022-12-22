using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(BlendAnimationStatesSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    internal partial struct UpdateAnimationStatesSystem : ISystem
    {
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
        private void OnUpdate_Safe(ref SystemState state)
        {
            new UpdateSingleClipStatesJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel();

            new CleanSingleClipStatesJob().ScheduleParallel();

            new UpdateLinearBlendStateMachineStatesJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel();

            new CleanLinearBlendStatesJob().ScheduleParallel();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUpdate_Unsafe(ref SystemState state)
        {
            var singleClipHandle = new UpdateSingleClipStatesJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel(state.Dependency);

            singleClipHandle = new CleanSingleClipStatesJob().ScheduleParallel(singleClipHandle);

            var linearBlendHandle = new UpdateLinearBlendStateMachineStatesJob
            {
                DeltaTime = SystemAPI.Time.DeltaTime,
            }.ScheduleParallel(state.Dependency);

            linearBlendHandle = new CleanLinearBlendStatesJob().ScheduleParallel(linearBlendHandle);

            state.Dependency = JobHandle.CombineDependencies(singleClipHandle, linearBlendHandle);
        }
    }
}