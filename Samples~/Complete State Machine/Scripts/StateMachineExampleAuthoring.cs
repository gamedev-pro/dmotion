using DMotion;
using DMotion.Authoring;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.CompleteStateMachine
{
    public struct StateMachineExampleOneShots : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> Clips;
        public BlobAssetReference<ClipEventsBlob> ClipEvents;
        public ushort SlashClipIndex;
        public ushort CircleSlashClipIndex;
    }

    public class StateMachineExampleAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationEventName startAttackEvent;
        [SerializeField] private AnimationEventName endAttackEvent;
        [SerializeField] private AnimationEventName footStepEvent;

        [SerializeField] private AnimationClipAsset slashClipAsset;
        [SerializeField] private AnimationClipAsset circleSlashClipAsset;

        private SmartBlobberHandle<SkeletonClipSetBlob> clipsHandle;
        private SmartBlobberHandle<ClipEventsBlob> clipEventsHandle;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new StateMachineExampleEvents()
            {
                StartAttackEventHash = startAttackEvent.Hash,
                EndAttackEventHash = endAttackEvent.Hash,
                FootStepEventHash = footStepEvent.Hash,
            });

            dstManager.AddComponentData(entity, new StateMachineExampleOneShots()
            {
                Clips = clipsHandle.Resolve(),
                ClipEvents = clipEventsHandle.Resolve(),
                SlashClipIndex = 0,
                CircleSlashClipIndex = 1
            });

            dstManager.AddComponent<AttackWindow>(entity);
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem)
        {
            clipsHandle = conversionSystem.RequestClipsBlob(animator, slashClipAsset, circleSlashClipAsset);
            clipEventsHandle =
                conversionSystem.RequestClipEventsBlob(animator.gameObject, slashClipAsset, circleSlashClipAsset);
        }
    }
}