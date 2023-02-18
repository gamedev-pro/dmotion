using DMotion.Authoring;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;

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


    class PerformanceTestsAuthoring : MonoBehaviour
    {
        public Animator Animator;
        public AnimationClipAsset OneShotClip;
    }

    class PerfomanceTestBaker : SmartBaker<PerformanceTestsAuthoring, PerformanceTestBakeItem>
    {
    }

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