using DMotion.Samples.Common;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.AnimationEvents
{
    //tag for this sample
    struct AnimationEventsSample : IComponentData
    {
    }

    class AnimationEventsSampleAuthoring : MonoBehaviour
    {
    }

    class AnimationEventsSampleBaker : Baker<AnimationEventsSampleAuthoring>
    {
        public override void Bake(AnimationEventsSampleAuthoring authoring)
        {
            AddComponent<AnimationEventsSample>();
        }
    }
}