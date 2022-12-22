using DMotion.Authoring;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DMotion.PerformanceTests
{
    public struct StressTestOneShotClip : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> Clips;
        public BlobAssetReference<ClipEventsBlob> ClipEvents;
        public short ClipIndex;
    }

    public struct LinearBlendDirection : IComponentData
    {
        public short Value;
    }

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class StateMachinePerformanceTestSystem : SystemBase
    {
        public static readonly ProfilerMarker Marker =
            ProfilingUtils.CreateAnimationMarker<StateMachinePerformanceTestSystem>(nameof(OnUpdate));

        protected override void OnUpdate()
        {
            var dt = SystemAPI.Time.DeltaTime;
            var integerPart = (uint)SystemAPI.Time.ElapsedTime + 1;
            var decimalPart = (float)(SystemAPI.Time.ElapsedTime + 1) - integerPart;
            var shouldSwitchStates = decimalPart < dt && integerPart % 2 == 0;
            var marker = Marker;

            Entities.ForEach((
                Entity e,
                ref LinearBlendDirection linearBlendDirection,
                ref PlayOneShotRequest playOneShotRequest,
                ref DynamicBuffer<FloatParameter> blendParameters,
                ref DynamicBuffer<BoolParameter> boolParameters,
                in StressTestOneShotClip oneShotClip) =>
            {
                using var scope = marker.Auto();
                blendParameters[0] = new FloatParameter
                {
                    Hash = blendParameters[0].Hash,
                    Value = math.clamp(blendParameters[0].Value + linearBlendDirection.Value * dt, 0, 1)
                };

                if (shouldSwitchStates)
                {
                    var rnd = Random.CreateFromIndex((uint)(e.Index + integerPart));
                    var prob = rnd.NextInt(0, 101);
                    if (prob < 30)
                    {
                        linearBlendDirection.Value *= -1;
                    }
                    else if (prob < 60)
                    {
                        boolParameters[0] = new BoolParameter
                        {
                            Hash = boolParameters[0].Hash,
                            Value = !boolParameters[0].Value
                        };
                    }
                    else
                    {
                        playOneShotRequest =
                            new PlayOneShotRequest(oneShotClip.Clips, oneShotClip.ClipEvents, oneShotClip.ClipIndex);
                    }
                }
            }).ScheduleParallel();
        }
    }

    class PerformanceTestsAuthoring : MonoBehaviour
    {
        public Animator Animator;
        public AnimationClipAsset OneShotClip;
    }
    
    class PerfomanceTestBaker : SmartBaker<PerformanceTestsAuthoring, PerformanceTestBakeItem>{}

    struct PerformanceTestBakeItem : ISmartBakeItem<PerformanceTestsAuthoring>
    {
        private SmartBlobberHandle<SkeletonClipSetBlob> clipsHandle;

        public bool Bake(PerformanceTestsAuthoring authoring, IBaker baker)
        {
            clipsHandle = baker.RequestCreateBlobAsset(authoring.Animator, authoring.OneShotClip);
            return true;
        }

        public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
        {
            AnimationStateMachineConversionUtils.AddOneShotSystemComponents(entityManager, entity);
            entityManager.AddComponentData(entity, new LinearBlendDirection { Value = 1 });
            entityManager.AddComponentData(entity, new StressTestOneShotClip
            {
                Clips = clipsHandle.Resolve(entityManager),
                ClipEvents = BlobAssetReference<ClipEventsBlob>.Null,
                ClipIndex = 0
            });
        }
    }
}