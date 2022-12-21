using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Authoring
{
    [DisallowMultipleComponent]
    public class PlayClipAuthoring : MonoBehaviour
    {
        public GameObject Owner;
        public Animator Animator;
        public SingleClipRefConvertData Clip;
        public bool Loop = true;

        public RootMotionMode RootMotionMode;
        public bool EnableEvents = true;

        public bool EnableSingleClipRequests = true;
    }

    public struct PlaySingleClipBakeData
    {
        public Entity Owner;
        public SmartBlobberHandle<SkeletonClipSetBlob> ClipsBlobHandle;
        public SmartBlobberHandle<ClipEventsBlob> ClipEventsBlobHandle;
        public bool Loop;
        public float Speed;
        public RootMotionMode RootMotionMode;
        public bool EnableEvents;
        public bool EnableSingleClipRequests;
    }
    
    public class PlayClipBaker : SmartBaker<PlayClipAuthoring, PlayClipBakeItem>{}

    public struct PlayClipBakeItem : ISmartBakeItem<PlayClipAuthoring>
    {
        public PlaySingleClipBakeData BakeData;

        public bool Bake(PlayClipAuthoring authoring, IBaker baker)
        {
            Assert.IsNotNull(authoring.Clip.Clip, $"Trying to play null clip ({authoring.gameObject.name})");
            if (authoring.Clip.Clip == null)
            {
                return false;
            }

            var clip = authoring.Clip;
            BakeData.Owner = baker.GetEntity(authoring.Owner);
            BakeData.ClipsBlobHandle = baker.RequestCreateBlobAsset(authoring.Animator, clip.Clip);
            BakeData.ClipEventsBlobHandle =
                baker.RequestCreateBlobAsset(clip.Clip);
            BakeData.Loop = authoring.Loop;
            BakeData.Speed = clip.Speed;
            BakeData.RootMotionMode = authoring.RootMotionMode;
            BakeData.EnableEvents = authoring.EnableEvents;
            BakeData.EnableSingleClipRequests = authoring.EnableSingleClipRequests;
            return true;
        }

        public void PostProcessBlobRequests(EntityManager entityManager, Entity entity)
        {
            if (BakeData.Owner == Entity.Null)
            {
                BakeData.Owner = entity;
            }

            AnimationStateMachineConversionUtils.AddSingleClipStateComponents(entityManager, BakeData.Owner, entity,
                BakeData.EnableEvents, BakeData.EnableSingleClipRequests, BakeData.RootMotionMode);

            var clipsBlob = BakeData.ClipsBlobHandle.Resolve(entityManager);
            var clipEventsBlob = BakeData.ClipEventsBlobHandle.Resolve(entityManager);

            var singleClipRef = new SingleClipRef(clipsBlob, clipEventsBlob, 0, BakeData.Speed);

            if (singleClipRef.IsValid)
            {
                if (BakeData.EnableSingleClipRequests)
                {
                    entityManager.SetComponentData(entity, PlaySingleClipRequest.New(singleClipRef));
                }
                else
                {
                    var singleClips = entityManager.GetBuffer<SingleClipState>(entity);
                    var animationStates = entityManager.GetBuffer<AnimationState>(entity);
                    var clipSamplers = entityManager.GetBuffer<ClipSampler>(entity);

                    var singleClipState = SingleClipStateUtils.New(singleClipRef.ClipIndex, singleClipRef.Speed,
                        BakeData.Loop, singleClipRef.Clips, singleClipRef.ClipEvents, ref singleClips,
                        ref animationStates,
                        ref clipSamplers);

                    entityManager.SetComponentData(entity, new AnimationStateTransitionRequest
                    {
                        AnimationStateId = (sbyte)singleClipState.AnimationStateId,
                        TransitionDuration = 0
                    });
                }
            }
        }
    }
}