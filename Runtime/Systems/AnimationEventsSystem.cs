using BovineLabs.Event.Systems;
using Unity.Entities;
using Unity.Transforms;

namespace DOTSAnimation
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(AnimationStateMachineSystem))]
    public partial class AnimationEventsSystem : SystemBase
    {
        private EventSystem eventSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            eventSystem = World.GetOrCreateSystem<EventSystem>();
        }

        protected override void OnUpdate()
        {
            new RaiseAnimationEventsJob()
            {
                Writer = eventSystem.CreateEventWriter<RaisedAnimationEvent>(),
            }.ScheduleParallel();
            eventSystem.AddJobHandleForProducer<RaisedAnimationEvent>(Dependency);
        }
    }
}