using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(BlendAnimationStatesSystem))]
    public partial class AnimationStateMachineSystem : SystemBase
    {
        internal static readonly ProfilerMarker Marker_UpdateStateMachineJob =
            new ProfilerMarker($"UpdateStateMachineJob");
        
        protected override void OnUpdate()
        {
            new UpdateStateMachineJob
            {
                Marker = Marker_UpdateStateMachineJob
            }.ScheduleParallel();
        }
    }
}