using DMotion.Authoring;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Assertions;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.PlayOneShot
{
    struct PlayOneShotExampleComponent : IComponentData
    {
        public SingleClipRef OneShotClipRef;
        public float TransitionDuration;
        public float NormalizedEndTime;
    }

    class PlayOneShotExampleAuthoring : MonoBehaviour
    {
        public Animator Animator;
        public SingleClipRefConvertData OneShotClip;
        public float TransitionDuration = 0.15f;
        [Range(0, 1)] public float NormalizedEndTime = 0.8f;
    }

    class PlayOneShotExampleBaker : SmartBaker<PlayOneShotExampleAuthoring, PlayOneShotExampleBakeItem>
    {
    }

    internal struct PlayOneShotExampleBakeItem : ISmartBakeItem<PlayOneShotExampleAuthoring>
    {
        public SmartBlobberHandle<SkeletonClipSetBlob> ClipsBlobHandle;
        public SmartBlobberHandle<ClipEventsBlob> ClipEventsBlobHandle;

        public float Speed;
        public float TransitionDuration;
        public float NormalizeEndTime;

        public bool Bake(PlayOneShotExampleAuthoring authoring, IBaker baker)
        {
            Assert.IsNotNull(authoring.OneShotClip.Clip, $"Missing one shot clip");
            ClipsBlobHandle = baker.RequestCreateBlobAsset(authoring.Animator, authoring.OneShotClip.Clip);
            ClipEventsBlobHandle =
                baker.RequestCreateBlobAsset(authoring.OneShotClip.Clip);
            Speed = authoring.OneShotClip.Speed;
            TransitionDuration = authoring.TransitionDuration;
            NormalizeEndTime = authoring.NormalizedEndTime;
            return true;
        }

        public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
        {
            AnimationStateMachineConversionUtils.AddOneShotSystemComponents(entityManager, entity);
            //Setup single clip refs
            var clipsBlob = ClipsBlobHandle.Resolve(entityManager);
            var clipEventsBlob = ClipEventsBlobHandle.Resolve(entityManager);
            entityManager.AddComponentData(entity,
                new PlayOneShotExampleComponent
                {
                    OneShotClipRef = new SingleClipRef(clipsBlob, clipEventsBlob, 0, Speed),
                    TransitionDuration = TransitionDuration,
                    NormalizedEndTime = NormalizeEndTime
                });
        }
    }
}