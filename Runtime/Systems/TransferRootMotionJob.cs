using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;
using UnityEngine;

namespace DMotion
{
    [BurstCompile]
    [WithOptions(EntityQueryOptions.FilterWriteGroup)]
    [WithAll(typeof(TransferRootMotionToOwner))]
    internal partial struct TransferRootMotionJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<RootDeltaTranslation> CfeDeltaPosition;
        [ReadOnly] public ComponentLookup<RootDeltaRotation> CfeDeltaRotation;
        internal ProfilerMarker Marker;

        public void Execute(ref Translation translation, ref Rotation rotation, in AnimatorOwner owner)
        {
            using var scope = Marker.Auto();
            var deltaPos = CfeDeltaPosition[owner.AnimatorEntity];
            var deltaRot = CfeDeltaRotation[owner.AnimatorEntity];
            rotation.Value = math.mul(deltaRot.Value, rotation.Value);

            translation.Value += deltaPos.Value;
        }
    }
}