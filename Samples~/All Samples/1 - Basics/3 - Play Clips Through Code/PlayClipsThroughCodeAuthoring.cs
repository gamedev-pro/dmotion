using System.Linq;
using DMotion.Authoring;
using DMotion.Samples.Common;
using Latios.Authoring;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Samples.PlayClipsThroughCode
{
    public class PlayClipsThroughCodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public GameObject Owner;
        public Animator Animator;
        public SingleClipRefConvertData WalkClip;
        public SingleClipRefConvertData RunClip;
        public float TransitionDuration = 0.15f;

        public RootMotionMode RootMotionMode;
        public bool EnableEvents = true;

        private SingleClipRefsConverter singleClipsConverter;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //create this sample's system
            DMotionSamplesUtils.AddSytemToPlayerUpdate<PlayClipsThroughCodeSystem>(dstManager);

            //Add single clip state components
            var ownerEntity = gameObject != Owner ? conversionSystem.GetPrimaryEntity(Owner) : entity;
            AnimationStateMachineConversionUtils.AddSingleClipStateComponents(dstManager, ownerEntity, entity,
                EnableEvents, RootMotionMode);

            //Setup single clip refs
            var clips = singleClipsConverter.ConvertClips().ToArray();
            if (clips.Length == 2)
            {
                dstManager.AddComponentData(entity,
                    new PlayClipsThroughCodeComponent
                    {
                        WalkClip = clips[0],
                        RunClip = clips[1],
                        TransitionDuration = TransitionDuration
                    });
            }
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem)
        {
            Assert.IsNotNull(WalkClip.Clip, $"Missing walk clip");
            Assert.IsNotNull(RunClip.Clip, $"Missing run clip");
            singleClipsConverter = new SingleClipRefsConverter(Animator, new[] { WalkClip, RunClip });
            singleClipsConverter.RequestBlobAssets(conversionSystem);
        }
    }
}