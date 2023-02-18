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

        public IntParameter(int hash)
        {
            Hash = hash;
            Value = 0;
        }
    }

    public struct BoolParameter : IBufferElementData, IStateMachineParameter<bool>
    {
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

        public BoolParameter(int hash)
        {
            Hash = hash;
            Value = false;
        }
    }

    public struct FloatParameter : IBufferElementData, IStateMachineParameter<float>
    {
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

        public FloatParameter(int hash)
        {
            Hash = hash;
            Value = 0;
        }
    }
}