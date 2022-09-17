using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace DMotion
{

    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PlayablesSystem))]
    internal partial class UpdateAnimationStatesSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var singleClipHandle = new UpdateSingleClipStateMachineStatesJob
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel(Dependency);

            singleClipHandle = new CleanSingleClipStatesJob().ScheduleParallel(singleClipHandle);
            
            var linearBlendHandle = new UpdateLinearBlendStateMachineStatesJob
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel(Dependency);

            linearBlendHandle = new CleanLinearBlendStatesJob().ScheduleParallel(linearBlendHandle);
            
            Dependency = JobHandle.CombineDependencies(singleClipHandle, linearBlendHandle);
        }
    }
}