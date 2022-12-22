using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(BlendAnimationStatesSystem))]
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct AnimationStateMachineSystem : ISystem
    {
        internal static readonly ProfilerMarker Marker_UpdateStateMachineJob =
            new ProfilerMarker($"UpdateStateMachineJob");
        
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new UpdateStateMachineJob
            {
                Marker = Marker_UpdateStateMachineJob
            }.ScheduleParallel();
        }
    }
}