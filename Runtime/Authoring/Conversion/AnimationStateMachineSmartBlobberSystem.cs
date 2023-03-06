using Latios.Authoring;
using Latios.Authoring.Systems;
using Unity.Burst;
using Unity.Entities;

namespace DMotion.Authoring
{
    public struct StateMachineBlobBakeData
    {
        internal StateMachineAsset StateMachineAsset;
    }

    public struct AnimationStateMachineSmartBlobberFilter : ISmartBlobberRequestFilter<StateMachineBlob>
    {
        public StateMachineAsset StateMachine;

        public bool Filter(IBaker baker, Entity blobBakingEntity)
        {
            if (StateMachine == null)
            {
                return false;
            }

            var stateMachineConverter = AnimationStateMachineConversionUtils.CreateConverter(StateMachine);
            baker.AddComponent(blobBakingEntity, stateMachineConverter);
            return true;
        }
    }

    [UpdateInGroup(typeof(SmartBlobberBakingGroup))]
    [BurstCompile]
    [DisableAutoCreation]
    internal partial struct AnimationStateMachineSmartBlobberSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            new SmartBlobberTools<StateMachineBlob>().Register(state.World);
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
            public void Execute(ref SmartBlobberResult result, in StateMachineBlobConverter stateMachineBlobConverter)
            {
                var blob = stateMachineBlobConverter.BuildBlob();
                result.blob = Unity.Entities.LowLevel.Unsafe.UnsafeUntypedBlobAssetReference.Create(blob);
            }
        }
    }
}