using Unity.Entities;
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
        
        protected override void OnUpdate()
        {
            new UpdateStateMachineJob
            {
                DeltaTime = Time.DeltaTime,
                Marker = Marker_UpdateStateMachineJob
            }.ScheduleParallel();
        }
    }
}