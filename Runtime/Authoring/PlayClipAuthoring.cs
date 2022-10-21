using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Authoring
{
    public class PlayClipAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public GameObject Owner;
        public Animator Animator;
        public AnimationClipAsset Clip;
        public float Speed = 1;
        public bool Loop = true;

        public RootMotionMode RootMotionMode;
        public bool EnableEvents = true;

        private SmartBlobberHandle<SkeletonClipSetBlob> clipsBlobHandle;
        private SmartBlobberHandle<ClipEventsBlob> clipEventsBlobHandle;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var clipsBlob = clipsBlobHandle.Resolve();
            var clipEventsBlob = clipEventsBlobHandle.Resolve();

            AnimationStateMachineConversionUtils.AddAnimationStateSystemComponents(dstManager, entity);
            AnimationStateMachineConversionUtils.AddOneShotSystemComponents(dstManager, entity);

            var singleClips = dstManager.AddBuffer<SingleClipState>(entity);
            var animationStates = dstManager.GetBuffer<AnimationState>(entity);
            var clipSamplers = dstManager.GetBuffer<ClipSampler>(entity);
            var singleClipState = SingleClipStateUtils.New(0, Speed, Loop, clipsBlob, clipEventsBlob, ref singleClips,
                ref animationStates,
                ref clipSamplers);

            dstManager.SetComponentData(entity,
                new AnimationStateTransitionRequest
                {
                    AnimationStateId = (sbyte)singleClipState.AnimationStateId,
                    TransitionDuration = 0
                });

            if (EnableEvents)
            {
                dstManager.GetOrCreateBuffer<RaisedAnimationEvent>(entity);
            }

            var ownerEntity = gameObject != Owner ? conversionSystem.GetPrimaryEntity(Owner) : entity;
            if (ownerEntity != entity)
            {
                AnimationStateMachineConversionUtils.AddAnimatorOwnerComponents(dstManager, ownerEntity, entity);
            }

            AnimationStateMachineConversionUtils.AddRootMotionComponents(dstManager, ownerEntity, entity,
                RootMotionMode);
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem)
        {
            clipsBlobHandle = conversionSystem.RequestClipsBlob(Animator, Clip);
            clipEventsBlobHandle = conversionSystem.RequestClipEventsBlob(Animator.gameObject, Clip);
        }
    }
}