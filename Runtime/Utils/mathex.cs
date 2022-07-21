using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    public static class mathex
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static quaternion delta(in quaternion from, in quaternion to)
        {
            var inv = math.inverse(from);
            return math.normalizesafe(math.mul(inv, to));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool approximately(in float a, in float b)
        {
            return a - b < math.EPSILON;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool iszero(in float a)
        {
            return a < math.EPSILON;
        }
    }
}