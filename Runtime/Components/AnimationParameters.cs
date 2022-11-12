using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    public interface IHasHash
    {
        int Hash { get; }
    }

    public interface IStateMachineParameter<T> : IHasHash
        where T : struct
    {
        T Value { get; set; }
    }

    public struct IntParameter : IBufferElementData, IStateMachineParameter<int>
    {
#if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
#endif
        public int Hash;
        public int Value;
        int IHasHash.Hash => Hash;
        int IStateMachineParameter<int>.Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Value = value;
        }

        public IntParameter(FixedString64Bytes name, int hash)
        {
#if UNITY_EDITOR || DEBUG
            Name = name;
#endif
            Hash = hash;
            Value = 0;
        }
    }

    public struct BoolParameter : IBufferElementData, IStateMachineParameter<bool>
    {
#if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
#endif
        public int Hash;
        public bool Value;

        int IHasHash.Hash => Hash;
        bool IStateMachineParameter<bool>.Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Value = value;
        }

        public BoolParameter(FixedString64Bytes name, int hash)
        {
#if UNITY_EDITOR || DEBUG
            Name = name;
#endif
            Hash = hash;
            Value = false;
        }
    }

    public struct FloatParameter : IBufferElementData, IStateMachineParameter<float>
    {
#if UNITY_EDITOR || DEBUG
        public FixedString64Bytes Name;
#endif
        public int Hash;
        public float Value;
        int IHasHash.Hash => Hash;
        float IStateMachineParameter<float>.Value
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Value;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Value = value;
        }

        public FloatParameter(FixedString64Bytes name, int hash)
        {
#if UNITY_EDITOR || DEBUG
            Name = name;
#endif
            Hash = hash;
            Value = 0;
        }
    }
}