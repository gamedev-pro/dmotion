using DMotion.Samples.Common;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.AnimationEvents
{
    public class AnimationEventsSampleAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            DMotionSamplesUtils.AddSytemToPlayerUpdate<AnimationEventsSampleSystem>(dstManager);
        }
    }
}