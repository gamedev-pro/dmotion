using DMotion.Authoring;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Samples.PlayClipsThroughCode
{
    struct PlayClipsThroughCodeComponent : IComponentData
    {
        public SingleClipRef WalkClip;
        public SingleClipRef RunClip;
        public float TransitionDuration;
    }

    class PlayClipsThroughCodeAuthoring : MonoBehaviour
    {
        public GameObject Owner;
        public Animator Animator;
        public SingleClipRefConvertData WalkClip = SingleClipRefConvertData.Default;
        public SingleClipRefConvertData RunClip = SingleClipRefConvertData.Default;
        public float TransitionDuration = 0.15f;
    }
    
    class PlayClipsThroughCodeBaker : SmartBaker<PlayClipsThroughCodeAuthoring, PlayClipsThroughCodeBakeItem>{}

    struct PlayClipsThroughCodeBakeItem : ISmartBakeItem<PlayClipsThroughCodeAuthoring>
    {
        public Entity Owner;
        public SmartBlobberHandle<SkeletonClipSetBlob> ClipsBlobHandle;
        public SmartBlobberHandle<ClipEventsBlob> ClipEventsBlobHandle;
        public float WalkClipSpeed;
        public float RunClipSpeed;
        public RootMotionMode RootMotionMode;
        public float TransitionDuration;
        public bool EnableEvents;

        public bool Bake(PlayClipsThroughCodeAuthoring authoring, IBaker baker)
        {
            Assert.IsNotNull(authoring.WalkClip.Clip, $"Missing walk clip");
            Assert.IsNotNull(authoring.RunClip.Clip, $"Missing run clip");

            var clips = new[] { authoring.WalkClip.Clip, authoring.RunClip.Clip };
            ClipsBlobHandle = baker.RequestCreateBlobAsset(authoring.Animator, clips);
            ClipEventsBlobHandle =
                baker.RequestCreateBlobAsset(clips);
            Owner = baker.GetEntity(authoring.Owner);
            TransitionDuration = authoring.TransitionDuration;
            WalkClipSpeed = authoring.WalkClip.Speed;
            RunClipSpeed = authoring.RunClip.Speed;
            return true;
        }

        public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
        {
            //Add single clip state components
            Owner = Owner == Entity.Null ? entity : Owner;
            AnimationStateMachineConversionUtils.AddSingleClipStateComponents(entityManager, Owner, entity,
                EnableEvents, true, RootMotionMode);

            //Setup single clip refs
            var clipsBlob = ClipsBlobHandle.Resolve(entityManager);
            var clipEventsBlob = ClipEventsBlobHandle.Resolve(entityManager);

            entityManager.AddComponentData(entity, new PlayClipsThroughCodeComponent
            {
                WalkClip = new SingleClipRef(clipsBlob, clipEventsBlob, 0, WalkClipSpeed),
                RunClip = new SingleClipRef(clipsBlob, clipEventsBlob, 1, RunClipSpeed),
                TransitionDuration = TransitionDuration
            });
        }
    }
}