using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public class AnimationStateMachineAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public GameObject Owner;
        public Animator Animator;
        public StateMachineAsset StateMachineAsset;
        private SmartBlobberHandle<SkeletonClipSetBlob> clipsBlobHandle;
        private SmartBlobberHandle<StateMachineBlob> stateMachineBlobHandle;
        private SmartBlobberHandle<ClipEventsBlob> clipEventsBlobHandle;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var stateMachineBlob = stateMachineBlobHandle.Resolve();
            var clipsBlob = clipsBlobHandle.Resolve();
            var clipEventsBlob = clipEventsBlobHandle.Resolve();

            var stateMachine = new AnimationStateMachine()
            {
                StateMachineBlob = stateMachineBlob,
                ClipsBlob = clipsBlob,
                ClipEventsBlob = clipEventsBlob,
                CurrentState = AnimationState.Null,
                NextState = AnimationState.Null,
                CurrentTransition = StateTransition.Null,
                Weight = 1
            };

            dstManager.AddComponentData(entity, stateMachine);
            var clipSamplers = dstManager.AddBuffer<ClipSampler>(entity);
            clipSamplers.Capacity = 10;

            if (StateMachineAsset.Clips.Any(c => c.Events.Length > 0))
            {
                dstManager.GetOrCreateBuffer<RaisedAnimationEvent>(entity);
            }
            
            var boolParameters = dstManager.AddBuffer<BoolParameter>(entity);
            for (ushort i = 0; i < StateMachineAsset.BoolParameters.Count; i++)
            {
                boolParameters.Add(new BoolParameter()
                {
                    Hash = StateMachineAsset.BoolParameters[i].Hash,
                    Value = false
                });
            }
            var floatParameters = dstManager.AddBuffer<BlendParameter>(entity);
            for (ushort i = 0; i < StateMachineAsset.FloatParameters.Count; i++)
            {
                floatParameters.Add(new BlendParameter()
                {
                    Hash = StateMachineAsset.FloatParameters[i].Hash,
                });
            }

            dstManager.AddComponentData(entity, PlayOneShotRequest.Null);
            dstManager.AddComponentData(entity, OneShotState.Null);

            var ownerEntity = conversionSystem.GetPrimaryEntity(Owner);
            dstManager.AddComponentData(ownerEntity, new AnimatorOwner() { AnimatorEntity = entity });
            dstManager.AddComponentData(entity, new AnimatorEntity() { Owner = ownerEntity});

            dstManager.AddComponentData(ownerEntity, new TransferRootMotion());
            dstManager.AddComponentData(entity, new RootDeltaTranslation());
            dstManager.AddComponentData(entity, new RootDeltaRotation());
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager, GameObjectConversionSystem conversionSystem)
        {
            clipsBlobHandle = conversionSystem.RequestClipsBlob(Animator, StateMachineAsset.Clips);
            stateMachineBlobHandle = conversionSystem.RequestStateMachineBlob(Animator.gameObject, new StateMachineBlobBakeData()
            {
                StateMachineAsset = StateMachineAsset
            });
            clipEventsBlobHandle = conversionSystem.RequestClipEventsBlob(Animator.gameObject, StateMachineAsset.Clips);
        }
    }
}