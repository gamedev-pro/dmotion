using Latios.Kinemation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DMotion.Samples
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    //must update after ClipSampling in order to have RootMotion deltas
    [UpdateAfter(typeof(ClipSamplingSystem))]
    public partial class CustomRootMotionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                //only works for when the SkeletonMes is the root object. Check TransferRootMotionJob.cs for an example of transfering root motion
                .WithAll<SkeletonRootTag>()
                //tag component to filter entities with custom motion
                .WithAll<CustomRootMotionComponent>()
                .ForEach((
                    ref Translation translation,
                    ref Rotation rotation,
                    in RootDeltaTranslation rootDeltaTranslation,
                    in RootDeltaRotation rootDeltaRotation
                ) =>
                {
                    //RootDeltaTranslation and RootDeltaRotation are calculated by DMotion in the ClipSamplingSystem, so you can just read them here
                    var deltaTranslation = -rootDeltaTranslation.Value;
                    translation.Value += deltaTranslation;
                    rotation.Value = math.mul(rootDeltaRotation.Value, rotation.Value);
                }).Schedule();
        }
    }
}