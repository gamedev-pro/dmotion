using Unity.Entities;

namespace DMotion
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