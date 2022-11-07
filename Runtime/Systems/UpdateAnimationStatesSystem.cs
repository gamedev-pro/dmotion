using System.Runtime.CompilerServices;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(BlendAnimationStatesSystem))]
    internal partial class UpdateAnimationStatesSystem : SystemBase
    {
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
        private void OnUpdate_Safe()
        {
             new UpdateSingleClipStatesJob
             {
                 DeltaTime = Time.DeltaTime,
             }.ScheduleParallel();
 
             new CleanSingleClipStatesJob().ScheduleParallel();
 
             new UpdateLinearBlendStateMachineStatesJob
             {
                 DeltaTime = Time.DeltaTime,
             }.ScheduleParallel();
 
             new CleanLinearBlendStatesJob().ScheduleParallel();
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void OnUpdate_Unsafe()
        {
            var singleClipHandle = new UpdateSingleClipStatesJob
            {
                DeltaTime = Time.DeltaTime,
            }.ScheduleParallel(Dependency);

            singleClipHandle = new CleanSingleClipStatesJob().ScheduleParallel(singleClipHandle);

            var linearBlendHandle = new UpdateLinearBlendStateMachineStatesJob
            {
                DeltaTime = Time.DeltaTime,
            }.ScheduleParallel(Dependency);

            linearBlendHandle = new CleanLinearBlendStatesJob().ScheduleParallel(linearBlendHandle);

            Dependency = JobHandle.CombineDependencies(singleClipHandle, linearBlendHandle);
        }
    }
}