using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    public struct RootDeltaTranslation : IComponentData
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