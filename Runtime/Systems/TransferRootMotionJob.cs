using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace DOTSAnimation
{
    [BurstCompile]
    [WithEntityQueryOptions(EntityQueryOptions.FilterWriteGroup)]
    [WithAll(typeof(TransferRootMotionToOwner))]
    internal partial struct TransferRootMotionJob : IJobEntity
    {
        [ReadOnly] public ComponentDataFromEntity<RootDeltaTranslation> CfeDeltaPosition;
        [ReadOnly] public ComponentDataFromEntity<RootDeltaRotation> CfeDeltaRotation;

        public void Execute(ref Translation translation, ref Rotation rotation, in AnimatorOwner owner)
        {
            var deltaPos = CfeDeltaPosition[owner.AnimatorEntity];
            var deltaRot = CfeDeltaRotation[owner.AnimatorEntity];
            rotation.Value = math.mul(deltaRot.Value, rotation.Value);

            translation.Value += deltaPos.Value;
        }
    }
}