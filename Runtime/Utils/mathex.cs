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
    }
}