using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [BurstCompile]
    [WithAll(typeof(SkeletonRootTag), typeof(ApplyRootMotionToEntity))]
    internal partial struct ApplyRootMotionToEntityJob : IJobEntity
    {
        internal ProfilerMarker Marker;
        internal void Execute(
            ref Translation translation,
            ref Rotation rotation,
            in RootDeltaTranslation rootDeltaTranslation,
            in RootDeltaRotation rootDeltaRotation
        )
        {
            using var scope = Marker.Auto();
            translation.Value += rootDeltaTranslation.Value;
            rotation.Value = math.mul(rootDeltaRotation.Value, rotation.Value);
        }
    }
}