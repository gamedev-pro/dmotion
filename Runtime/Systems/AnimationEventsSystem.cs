using Unity.Entities;
using Unity.Transforms;

namespace DOTSAnimation
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(AnimationStateMachineSystem))]
    public partial class AnimationEventsSystem : SystemBase
    {
        private EntityCommandBufferSystem ecbSystem;
        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer().AsParallelWriter();
            new RaiseAnimationEventsJob()
            {
                DeltaTime = Time.DeltaTime,
                Ecb = ecb
            }.ScheduleParallel();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}