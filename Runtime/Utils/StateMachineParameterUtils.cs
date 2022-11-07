using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    public struct StateMachineParameterRef<TBuffer, TValue>
        where TBuffer : struct, IStateMachineParameter<TValue>
        where TValue : struct
    {
        public sbyte Index;

        public void SetValue(DynamicBuffer<TBuffer> parameters, TValue value)
        {
            if (Index >= 0 && Index < parameters.Length)
            {
                var p = parameters[Index];
                p.Value = value;
                parameters[Index] = p;
            }
        }

        public bool TryGetValue(DynamicBuffer<TBuffer> parameters, out TValue value)
        {
            if (Index >= 0 && Index < parameters.Length)
            {
                value = parameters[Index].Value;
                return true;
            }

            value = default;
            return false;
        }
    }

    [BurstCompile]
    public static class StateMachineParameterUtils
    {
        public static int HashToIndex<T>(this DynamicBuffer<T> parameters, int hash)
            where T : struct, IHasHash
        {
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].Hash == hash)
                {
                    return i;
                }
            }

            return -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetHashCode(string name)
        {
            return ((FixedString64Bytes) name).GetHashCode();
        }

        public static void SetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, int hash, TValue value)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            var index = parameters.HashToIndex(hash);
            if (index >= 0)
            {
                var p = parameters[index];
                p.Value = value;
                parameters[index] = p;
            }
        }
        
        public static void SetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters,
            FixedString64Bytes name, TValue value)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            var hash = name.GetHashCode();
            parameters.SetValue(hash, value);
        }

        public static bool TryGetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, int hash,
            out TValue value)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            var index = parameters.HashToIndex(hash);
            if (index >= 0)
            {
                value = parameters[index].Value;
                return true;
            }

            value = default;
            return false;
        }

        public static bool TryGetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, FixedString64Bytes name,
            out TValue value)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            var hash = name.GetHashCode();
            return parameters.TryGetValue(hash, out value);
        }
        
        public static TValue GetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, int hash)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            parameters.TryGetValue<TBuffer, TValue>(hash, out var value);
            return value;
        }
        
        // public static TValue GetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, FixedString64Bytes name)
        //     where TBuffer : struct, IStateMachineParameter<TValue>
        //     where TValue : struct
        // {
        //     var hash = name.GetHashCode();
        //     return parameters.GetValue<TBuffer, TValue>(hash);
        // }

        public static StateMachineParameterRef<TBuffer, TValue> CreateRef<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters,
            int hash)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            return new StateMachineParameterRef<TBuffer, TValue>()
            {
                Index = (sbyte) parameters.HashToIndex(hash)
            };
        }
        
        public static StateMachineParameterRef<TBuffer, TValue> CreateRef<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters,
            FixedString64Bytes name)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            var hash = name.GetHashCode();
            return parameters.CreateRef<TBuffer, TValue>(hash);
        }
    }
}