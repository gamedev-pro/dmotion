using BovineLabs.Event.Systems;
using Unity.Entities;
using Unity.Transforms;

namespace DOTSAnimation
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(AnimationStateMachineSystem))]
    public partial class AnimationEventsSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            new RaiseAnimationEventsJob()
            {
            }.ScheduleParallel();
        }
    }
}