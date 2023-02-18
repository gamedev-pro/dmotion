using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples
{
    public struct CustomRootMotionComponent : IComponentData
    {
    }

    public class CustomRootMotionComponentAuthoring : MonoBehaviour
    {
    }

    public class CustomRootMotionComponentBaker : Baker<CustomRootMotionComponentAuthoring>
    {
        public override void Bake(CustomRootMotionComponentAuthoring authoring)
        {
            AddComponent<CustomRootMotionComponent>();
        }
    }
}