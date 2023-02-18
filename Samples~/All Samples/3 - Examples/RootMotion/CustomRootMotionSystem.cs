using Latios.Kinemation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace DMotion.Samples
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    //must update after ClipSampling in order to have RootMotion deltas
    [UpdateAfter(typeof(ClipSamplingSystem))]
    [RequireMatchingQueriesForUpdate]
    public partial struct CustomRootMotionSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (translation, rotation, rootDeltaTranslation, rootDeltaRotation) in SystemAPI
                         .Query<RefRW<Translation>, RefRW<Rotation>, RootDeltaTranslation, RootDeltaRotation>()
                         .WithAll<CustomRootMotionComponent>()
                         .WithAll<SkeletonRootTag>())
            {
                //RootDeltaTranslation and RootDeltaRotation are calculated by DMotion in the ClipSamplingSystem, so you can just read them here
                var deltaTranslation = -rootDeltaTranslation.Value;
                translation.ValueRW.Value += deltaTranslation;
                rotation.ValueRW.Value = math.mul(rootDeltaRotation.Value, rotation.ValueRW.Value);
            }
        }
    }
}