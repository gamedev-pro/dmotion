using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEditor;

namespace DMotion
{
    [BurstCompile]
    public static class StateMachineParameterUtils
    {
        public static int HashToIndex(this DynamicBuffer<BoolParameter> parameters, int hash)
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
        
        public static void SetParameter(this DynamicBuffer<BoolParameter> parameters, int hash, bool value)
        {
            var index = parameters.HashToIndex(hash);
            if (index >= 0)
            {
                var p = parameters[index];
                p.Value = value;
                parameters[index] = p;
            }
        }


        public static void SetParameter(this DynamicBuffer<BoolParameter> parameters, FixedString32Bytes name, bool value)
        {
            var hash = name.GetHashCode();
            parameters.SetParameter(hash, value);
        }

        public static bool TryGetValue(this DynamicBuffer<BoolParameter> parameters, int hash, out bool value)
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
        public static bool TryGetValue(this DynamicBuffer<BoolParameter> parameters, FixedString32Bytes name, out bool value)
        {
            var hash = name.GetHashCode();
            return parameters.TryGetValue(hash, out value);
        }
        
        public static int HashToIndex(this DynamicBuffer<BlendParameter> parameters, int hash)
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
        
        public static void SetParameter(this DynamicBuffer<BlendParameter> parameters, int hash, float value)
        {
            var index = parameters.HashToIndex(hash);
            if (index >= 0)
            {
                var p = parameters[index];
                p.Value = value;
                parameters[index] = p;
            }
        }


        public static void SetParameter(this DynamicBuffer<BlendParameter> parameters, FixedString32Bytes name, float value)
        {
            var hash = name.GetHashCode();
            parameters.SetParameter(hash, value);
        }
        
        public static void IncrementParameter(this DynamicBuffer<BlendParameter> parameters, int hash, float increment)
        {
            var index = parameters.HashToIndex(hash);
            if (index >= 0)
            {
                var p = parameters[index];
                p.Value += increment;
                parameters[index] = p;
            }
        }

        public static bool TryGetValue(this DynamicBuffer<BlendParameter> parameters, int hash, out float value)
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
        public static bool TryGetValue(this DynamicBuffer<BlendParameter> parameters, FixedString32Bytes name, out float value)
        {
            var hash = name.GetHashCode();
            return parameters.TryGetValue(hash, out value);
        }

        public static int GetHashCode(string name)
        {
            return name.GetHashCode();
        }
    }
}