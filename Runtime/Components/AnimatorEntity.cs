using Unity.Entities;

namespace DMotion
{
    public struct AnimatorEntity : IComponentData
    {
        public Entity Owner;
    }
    public struct AnimatorOwner : IComponentData
    {
        public Entity AnimatorEntity;
    }
}