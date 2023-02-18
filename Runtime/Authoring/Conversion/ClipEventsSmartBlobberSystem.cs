using Latios.Authoring;
using Latios.Authoring.Systems;
using Unity.Burst;
using Unity.Entities;

namespace DMotion.Authoring
{
    public struct ClipEventSmartBlobberFilter : ISmartBlobberRequestFilter<ClipEventsBlob>
    {
        public AnimationClipAsset[] Clips;

        public bool Filter(IBaker baker, Entity blobBakingEntity)
        {
            if (Clips == null)
            {
                return false;
            }

            var converter = ClipEventsAuthoringUtils.CreateConverter(Clips);
            baker.AddComponent(blobBakingEntity, converter);
            return true;
        }
    }
    
    [UpdateInGroup(typeof(SmartBlobberBakingGroup))]
    [BurstCompile]
    internal partial struct ClipEventsSmartBlobberSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            new SmartBlobberTools<ClipEventsBlob>().Register(state.World);
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            new BuildBlobJob().ScheduleParallel();
        }

        [WithOptions(EntityQueryOptions.IncludeDisabledEntities | EntityQueryOptions.IncludePrefab)]
        [BurstCompile]
        partial struct BuildBlobJob : IJobEntity
        {
            public void Execute(ref SmartBlobberResult result, in ClipEventsBlobConverter converter)
            {
                var blob = converter.BuildBlob();
                result.blob = Unity.Entities.LowLevel.Unsafe.UnsafeUntypedBlobAssetReference.Create(blob);
            }
        }
    }
}