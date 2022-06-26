using Unity.Entities;

namespace DOTSAnimation
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