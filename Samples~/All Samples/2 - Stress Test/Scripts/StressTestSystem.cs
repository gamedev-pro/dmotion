using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DMotion.StressTest
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial class StressTestSystem : SystemBase
    {
        private EntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<StressTestSpawner>();
            RequireSingletonForUpdate<StressTestSampleActive>();
        }

        protected override void OnUpdate()
        {
            EntityManager.CompleteAllJobs();
            var ecb = ecbSystem.CreateCommandBuffer();

            Entities
                .ForEach((Entity entity, ref StressTestSpawner spawner) =>
                {
                    var spawnResolution = (int)math.ceil(math.sqrt(spawner.SkeletonsCount));
                    var shiftPosition = spawnResolution * 0.5f * spawner.Spacing;

                    var spawnCounter = 0;
                    for (var x = 0; x < spawnResolution; x++)
                    {
                        for (var y = 0; y < spawnResolution; y++)
                        {
                            var spawnedPrefab = ecb.Instantiate(spawner.SkeletonPrefab);
                            ecb.SetComponent(spawnedPrefab, new Translation { Value = new float3(x * spawner.Spacing - shiftPosition, 0f, y * spawner.Spacing - shiftPosition) });
                            spawnCounter++;
                            if (spawnCounter >= spawner.SkeletonsCount)
                            {
                                break;
                            }
                        }

                        if (spawnCounter >= spawner.SkeletonsCount)
                        {
                            break;
                        }
                    }

                    ecb.RemoveComponent<StressTestSpawner>(entity);
                }).Run();

            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}