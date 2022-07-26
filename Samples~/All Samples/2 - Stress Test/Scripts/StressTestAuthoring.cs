using DMotion.Authoring;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;

namespace DMotion.StressTest
{
    public class StressTestAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public Animator Animator;
        public AnimationClipAsset OneShotClip;

        private SmartBlobberHandle<SkeletonClipSetBlob> clipsHandle;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new LinearBlendDirection() { Value = 1 });
            dstManager.AddComponentData(entity, new StressTestOneShotClip()
            {
                Clips = clipsHandle.Resolve(),
                ClipEvents = BlobAssetReference<ClipEventsBlob>.Null,
                ClipIndex = 0
            });
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager, GameObjectConversionSystem conversionSystem)
        {
            clipsHandle = conversionSystem.RequestClipsBlob(Animator, OneShotClip);
        }
    }
}