using DMotion.Samples.Common;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.StateMachine
{
    public class SetParametersThroughCodeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            DMotionSamplesUtils.AddSytemToPlayerUpdate<SetParametersThroughCodeSystem>(dstManager);
        }
    }
}