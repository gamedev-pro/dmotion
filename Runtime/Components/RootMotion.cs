using Unity.Entities;
using Unity.Mathematics;

namespace DMotion
{
    public struct RootDeltaTranslation : IComponentData
    {
        public float3 Value;
    }
    
    public struct RootDeltaRotation : IComponentData
    {
        public quaternion Value;
    }

    internal struct ApplyRootMotionToEntity : IComponentData
    {
    }

    internal struct TransferRootMotionToOwner : IComponentData
    {
    }
}