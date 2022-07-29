using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DMotion.ComparisonTest
{
    public class AnimatorSpawner : IComponentData
    {
        public GameObject AnimatorPrefab;
        public int Count;
        public float Spacing;
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    public partial class SpawnAnimatorsSystem : SystemBase
    {
        private EntityCommandBufferSystem ecbSystem;

        protected override void OnCreate()
        {
            base.OnCreate();
            ecbSystem = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
            RequireSingletonForUpdate<AnimatorSpawner>();
            RequireSingletonForUpdate<AnimatorComparisonSampleActive>();
        }

        protected override void OnUpdate()
        {
            EntityManager.CompleteAllJobs();
            var ecb = ecbSystem.CreateCommandBuffer();

            Entities
                .ForEach((Entity entity, AnimatorSpawner spawner) =>
                {
                    var spawnResolution = (int)math.ceil(math.sqrt(spawner.Count));
                    var shiftPosition = spawnResolution * 0.5f * spawner.Spacing;

                    var spawnCounter = 0;
                    for (var x = 0; x < spawnResolution; x++)
                    {
                        for (var y = 0; y < spawnResolution; y++)
                        {
                            var pos = new float3(x * spawner.Spacing - shiftPosition, 0f,
                                y * spawner.Spacing - shiftPosition);
                            Object.Instantiate(spawner.AnimatorPrefab, pos, Quaternion.identity);
                            spawnCounter++;
                            if (spawnCounter >= spawner.Count)
                            {
                                break;
                            }
                        }

                        if (spawnCounter >= spawner.Count)
                        {
                            break;
                        }
                    }

                    ecb.RemoveComponent<AnimatorSpawner>(entity);
                }).WithoutBurst().Run();

            ecbSystem.AddJobHandleForProducer(Dependency);
        }
    }
}