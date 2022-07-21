using Unity.Entities;

namespace DOTSAnimation
{
    public struct BoolParameter : IBufferElementData
    {
        public int Hash;
        public bool Value;
    }
    
    public struct BlendParameter : IBufferElementData
    {
        public int Hash;
        public float Value;
    }
}