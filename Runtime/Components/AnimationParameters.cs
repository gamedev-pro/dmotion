using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    public interface IHasHash
    {
        public int Hash { get; }
    }
    public struct IntParameter : IBufferElementData, IHasHash
    {
         #if UNITY_EDITOR || DEBUG
         public FixedString64Bytes Name;
         #endif
         public int Hash;
         public int Value;
        int IHasHash.Hash => Hash;
         public IntParameter(FixedString64Bytes name, int hash)
         {
 #if UNITY_EDITOR || DEBUG
             Name = name;
 #endif
             Hash = hash;
             Value = 0;
         }       
    }
    
    public struct BoolParameter : IBufferElementData, IHasHash
    {
        #if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
        #endif
        public int Hash;
        public bool Value;

        int IHasHash.Hash => Hash;
        public BoolParameter(FixedString64Bytes name, int hash)
        {
#if UNITY_EDITOR || DEBUG
            Name = name;
#endif
            Hash = hash;
            Value = false;
        }
    }
    
    public struct BlendParameter : IBufferElementData, IHasHash
    {
        #if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
        #endif
        public int Hash;
        public float Value;
        int IHasHash.Hash => Hash;
        
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