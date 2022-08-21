using DMotion.Authoring;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace DMotion.PerformanceTests
{
    public struct StressTestOneShotClip : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> Clips;
        public BlobAssetReference<ClipEventsBlob> ClipEvents;
        public short ClipIndex;
    }
    
    public struct LinearBlendDirection : IComponentData
    {
        public short Value;
    }
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial class UpdateStateMachines : SystemBase
    {
        protected override void OnUpdate()
        {
            var dt = Time.DeltaTime;
            var integerPart = (uint)Time.ElapsedTime + 1;
            var decimalPart = (float) (Time.ElapsedTime + 1) - integerPart;
            var shouldSwitchStates = decimalPart < dt && integerPart % 2 == 0;
            
            Entities.ForEach((
                Entity e,
                ref LinearBlendDirection linearBlendDirection,
                ref PlayOneShotRequest playOneShotRequest,
                ref DynamicBuffer<BlendParameter> blendParameters,
                ref DynamicBuffer<BoolParameter> boolParameters,
                in StressTestOneShotClip oneShotClip) =>
            {
                blendParameters[0] = new BlendParameter
                {
                    Hash = blendParameters[0].Hash,
                    Value = math.clamp(blendParameters[0].Value + linearBlendDirection.Value*dt, 0, 1)
                };
            
                if (shouldSwitchStates)
                {
                    var rnd = Random.CreateFromIndex((uint)(e.Index + integerPart));
                    var prob = rnd.NextInt(0, 101);
                    if (prob < 30)
                    {
                        linearBlendDirection.Value *= -1;
                    }
                    else if (prob < 60)
                    {
                        boolParameters[0] = new BoolParameter
                        {
                            Hash = boolParameters[0].Hash,
                            Value = !boolParameters[0].Value
                        };
                    }
                    else
                    {
                        playOneShotRequest =
                            new PlayOneShotRequest(oneShotClip.Clips, oneShotClip.ClipEvents, oneShotClip.ClipIndex);
                    }
                }
            }).ScheduleParallel();
        }
    }

    public class PerformanceTestsAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public Animator Animator;
        public AnimationClipAsset OneShotClip;

        private SmartBlobberHandle<SkeletonClipSetBlob> clipsHandle;
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new LinearBlendDirection { Value = 1 });
            dstManager.AddComponentData(entity, new StressTestOneShotClip
            {
                Clips = clipsHandle.Resolve(),
                ClipEvents = BlobAssetReference<ClipEventsBlob>.Null,
                ClipIndex = 0
            });
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager, GameObjectConversionSystem conversionSystem)
        {
            clipsHandle = conversionSystem.RequestClipsBlob(Animator, OneShotClip);
        }
    }
}