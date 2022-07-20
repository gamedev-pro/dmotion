using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    internal struct RootPreviousTranslation : IComponentData
    {
        internal float3 Value;
    }
    internal struct RootPreviousRotation : IComponentData
    {
        internal quaternion Value;
    }
    
    internal struct RootTranslation : IComponentData
    {
        internal float3 Value;
    }
    
    internal struct RootRotation : IComponentData
    {
        internal quaternion Value;
    }

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