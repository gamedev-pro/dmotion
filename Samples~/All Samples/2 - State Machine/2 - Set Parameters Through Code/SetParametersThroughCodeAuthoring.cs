using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.StateMachine
{
    struct SetParametersThroughCodeSample : IComponentData{}
    class SetParametersThroughCodeAuthoring : MonoBehaviour
    {
    }

    class SetParametersThroughCodeBaker : Baker<SetParametersThroughCodeAuthoring>
    {
        public override void Bake(SetParametersThroughCodeAuthoring authoring)
        {
            AddComponent<SetParametersThroughCodeSample>();
        }
    }
}