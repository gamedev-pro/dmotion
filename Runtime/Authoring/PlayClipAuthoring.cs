using System.Linq;
using Latios.Authoring;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Authoring
{
    public class PlayClipAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public GameObject Owner;
        public Animator Animator;
        public SingleClipRefConvertData Clip;
        public bool Loop = true;

        public RootMotionMode RootMotionMode;
        public bool EnableEvents = true;

        public bool EnableSingleClipRequests = true;

        private SingleClipRefsConverter singleClipsConverter;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var ownerEntity = gameObject != Owner ? conversionSystem.GetPrimaryEntity(Owner) : entity;
            AnimationStateMachineConversionUtils.AddSingleClipStateComponents(dstManager, ownerEntity, entity,
                EnableEvents, EnableSingleClipRequests, RootMotionMode);
            var singleClipRef = singleClipsConverter.ConvertClips().FirstOrDefault();
            if (singleClipRef.IsValid)
            {
                if (EnableSingleClipRequests)
                {
                    dstManager.SetComponentData(entity, PlaySingleClipRequest.New(singleClipRef));
                }
                else
                {
                    var singleClips = dstManager.GetBuffer<SingleClipState>(entity);
                    var animationStates = dstManager.GetBuffer<AnimationState>(entity);
                    var clipSamplers = dstManager.GetBuffer<ClipSampler>(entity);

                    var singleClipState = SingleClipStateUtils.New(singleClipRef.ClipIndex, singleClipRef.Speed,
                        Loop, singleClipRef.Clips, singleClipRef.ClipEvents, ref singleClips,
                        ref animationStates,
                        ref clipSamplers);

                    dstManager.SetComponentData(entity, new AnimationStateTransitionRequest()
                    {
                        AnimationStateId = (sbyte)singleClipState.AnimationStateId,
                        TransitionDuration = 0
                    });
                }
            }
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem)
        {
            Assert.IsNotNull(Clip.Clip, $"Trying to play null clip ({gameObject.name})");
            singleClipsConverter = new SingleClipRefsConverter(Animator, new[] { Clip });
            singleClipsConverter.RequestBlobAssets(conversionSystem);
        }
    }
}