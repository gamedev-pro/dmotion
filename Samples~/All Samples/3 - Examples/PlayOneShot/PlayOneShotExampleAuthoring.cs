using System.Linq;
using DMotion.Authoring;
using DMotion.Samples.Common;
using Latios.Authoring;
using Unity.Assertions;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.PlayOneShot
{
    public struct PlayOneShotExampleComponent : IComponentData
    {
        public SingleClipRef OneShotClipRef;
        public float TransitionDuration;
        public float NormalizedEndTime;
    }
    public class PlayOneShotExampleAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public Animator Animator;
        public SingleClipRefConvertData OneShotClip;
        public float TransitionDuration = 0.15f;
        [Range(0, 1)] public float NormalizedEndTime = 0.8f;

        private SingleClipRefsConverter singleClipsConverter;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //create this sample's system
            DMotionSamplesUtils.AddSytemToPlayerUpdate<PlayOneShotExampleSystem>(dstManager);

            AnimationStateMachineConversionUtils.AddOneShotSystemComponents(dstManager, entity);

            //Setup single clip refs
            var clips = singleClipsConverter.ConvertClips().ToArray();
            if (clips.Length == 1)
            {
                dstManager.AddComponentData(entity,
                    new PlayOneShotExampleComponent
                    {
                        OneShotClipRef = clips[0],
                        TransitionDuration = TransitionDuration,
                        NormalizedEndTime = NormalizedEndTime
                    });
            }
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem)
        {
            Assert.IsNotNull(OneShotClip.Clip, $"Missing one shot clip");
            singleClipsConverter = new SingleClipRefsConverter(Animator, new[] { OneShotClip });
            singleClipsConverter.RequestBlobAssets(conversionSystem);
        }
    }
}