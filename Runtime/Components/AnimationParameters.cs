using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    public struct BoolParameter : IBufferElementData
    {
        #if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
        #endif
        public int Hash;
        public bool Value;
    }
    
    public struct BlendParameter : IBufferElementData
    {
        #if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
        #endif
        public int Hash;
        public float Value;
    }
}