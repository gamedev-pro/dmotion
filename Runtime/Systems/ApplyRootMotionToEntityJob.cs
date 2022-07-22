using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DMotion
{
    [BurstCompile]
    [WithAll(typeof(SkeletonRootTag), typeof(ApplyRootMotionToEntity))]
    internal partial struct ApplyRootMotionToEntityJob : IJobEntity
    {
        internal void Execute(
            ref Translation translation,
            ref Rotation rotation,
            in RootDeltaTranslation rootDeltaTranslation,
            in RootDeltaRotation rootDeltaRotation
        )
        {
            translation.Value += rootDeltaTranslation.Value;
            rotation.Value = math.mul(rootDeltaRotation.Value, rotation.Value);
        }
    }
}