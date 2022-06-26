using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    public struct RootDeltaPosition : IComponentData
    {
        public float3 Value;
    }
    
    public struct RootDeltaRotation : IComponentData
    {
        public quaternion Value;
    }

    public struct TransferRootMotion : IComponentData
    {
    }
}