using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(ClipSamplingSystem))]
    public partial class AnimationStateMachineSystem : SystemBase
    {
        internal static readonly ProfilerMarker Marker_UpdateStateMachineJob =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(UpdateStateMachineJob));
        
        internal static readonly ProfilerMarker Marker_UpdateSingleClips =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(UpdateSingleClipStateMachineStatesJob));
        internal static readonly ProfilerMarker Marker_UpdateLinearBlends =
            ProfilingUtils.CreateAnimationMarker<AnimationStateMachineSystem>(nameof(UpdateLinearBlendStateMachineStatesJob));
        protected override void OnUpdate()
        {
            new UpdateStateMachineJob
            {
                Marker = Marker_UpdateStateMachineJob
            }.ScheduleParallel();

            new UpdateSingleClipStateMachineStatesJob
            {
                DeltaTime = Time.DeltaTime,
                Marker = Marker_UpdateSingleClips
            }.ScheduleParallel();
            
            new UpdateLinearBlendStateMachineStatesJob
            {
                DeltaTime = Time.DeltaTime,
                Marker = Marker_UpdateLinearBlends
            }.ScheduleParallel();
        }
    }
}