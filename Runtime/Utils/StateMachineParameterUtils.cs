using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
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

        public static int GetHashCode(string name)
        {
            return name.GetHashCode();
        }

        public static void SetParameter<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, int hash, TValue value)
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

        public static void SetParameter<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, FixedString32Bytes name, TValue value)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            var hash = name.GetHashCode();
            parameters.SetParameter(hash, value);
        }

        public static bool TryGetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, int hash, out TValue value)
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

        public static bool TryGetValue<TBuffer, TValue>(this DynamicBuffer<TBuffer> parameters, FixedString32Bytes name,
            out TValue value)
            where TBuffer : struct, IStateMachineParameter<TValue>
            where TValue : struct
        {
            var hash = name.GetHashCode();
            return parameters.TryGetValue(hash, out value);
        }
    }
}