#if NETCODE_PROJECT
using Unity.Entities;
using Unity.NetCode;
namespace DMotion.Samples
{
    public partial class AddNetworkStreamInGame : SystemBase
    {
        private EntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var ecb = ecbSystem.CreateCommandBuffer();
            Entities
                .WithAll<NetworkStreamConnection>()
                .WithNone<NetworkStreamInGame>()
                .ForEach((Entity e) =>
                {
                    ecb.AddComponent<NetworkStreamInGame>(e);
                }).Run();
            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}
#endif
