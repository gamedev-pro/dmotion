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

    class PlayClipsThroughCodeBaker : SmartBaker<PlayClipsThroughCodeAuthoring, PlayClipsThroughCodeBakeItem>
    {
    }

    struct PlayClipsThroughCodeBakeItem : ISmartBakeItem<PlayClipsThroughCodeAuthoring>
    {
        //SmartBlobberHandles hold a request to a BlobAsset, that can be resolved later in PostProcessBlobRequest
        private SmartBlobberHandle<SkeletonClipSetBlob> clipsBlobHandle;
        private SmartBlobberHandle<ClipEventsBlob> clipEventsBlobHandle;

        public float WalkClipSpeed;
        public float RunClipSpeed;
        public float TransitionDuration;

        /// <summary>
        /// In the Bake function, you can do the following:
        /// 1. Add components that *do not require BlobAssets* to your entity, using the IBaker
        /// 2. Request conversion of any blob assets you need
        /// 3. Cache data you will need later on PostProcessBlobRequest (in this case we cache TransitionDuration)
        /// </summary>
        public bool Bake(PlayClipsThroughCodeAuthoring authoring, IBaker baker)
        {
            Assert.IsNotNull(authoring.WalkClip.Clip, $"Missing walk clip");
            Assert.IsNotNull(authoring.RunClip.Clip, $"Missing run clip");

            //Add single clip components to your entity. Those are required to play individual clips
            AnimationStateMachineConversionUtils.AddSingleClipStateComponents(baker, baker.GetEntity(authoring.Owner),
                baker.GetEntity(),
                false, true, RootMotionMode.Disabled);

            //Store data we will need on PostProcessBlobRequest
            TransitionDuration = authoring.TransitionDuration;
            WalkClipSpeed = authoring.WalkClip.Speed;
            RunClipSpeed = authoring.RunClip.Speed;

            //Request clips conversion the clips will be ready when PostProcessBlobRequest executes
            var clips = new[] { authoring.WalkClip.Clip, authoring.RunClip.Clip };
            clipsBlobHandle = baker.RequestCreateBlobAsset(authoring.Animator, clips);
            clipEventsBlobHandle =
                baker.RequestCreateBlobAsset(clips);

            return true;
        }

        public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
        {
            //Resolve the blob assets
            var clipsBlob = clipsBlobHandle.Resolve(entityManager);
            var clipEventsBlob = clipEventsBlobHandle.Resolve(entityManager);

            //Add the component referencing animation clips
            entityManager.AddComponentData(entity, new PlayClipsThroughCodeComponent
            {
                WalkClip = new SingleClipRef(clipsBlob, clipEventsBlob, 0, WalkClipSpeed),
                RunClip = new SingleClipRef(clipsBlob, clipEventsBlob, 1, RunClipSpeed),
                TransitionDuration = TransitionDuration
            });
        }
    }
}