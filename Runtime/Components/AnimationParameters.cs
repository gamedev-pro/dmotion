using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    public struct IntParameter : IBufferElementData
    {
         #if UNITY_EDITOR || DEBUG
         public FixedString64Bytes Name;
         #endif
         public int Hash;
         public int Value;
         public IntParameter(FixedString64Bytes name, int hash)
         {
 #if UNITY_EDITOR || DEBUG
             Name = name;
 #endif
             Hash = hash;
             Value = 0;
         }       
    }
    
    public struct BoolParameter : IBufferElementData
    {
        #if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
        #endif
        public int Hash;
        public bool Value;
        public BoolParameter(FixedString64Bytes name, int hash)
        {
#if UNITY_EDITOR || DEBUG
            Name = name;
#endif
            Hash = hash;
            Value = false;
        }
    }
    
    public struct BlendParameter : IBufferElementData
    {
        #if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
        #endif
        public int Hash;
        public float Value;
        
        public BlendParameter(FixedString64Bytes name, int hash)
        {
#if UNITY_EDITOR || DEBUG
            Name = name;
#endif
            Hash = hash;
            Value = 0;
        }
    }
}