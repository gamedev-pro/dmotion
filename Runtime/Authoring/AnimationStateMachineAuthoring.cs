using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public class AnimationStateMachineAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public GameObject Owner;
        public Animator Animator;
        public StateMachineAsset StateMachineAsset;
        private SmartBlobberHandle<SkeletonClipSetBlob> clipsBlobHandle;
        private SmartBlobberHandle<StateMachineBlob> stateMachinBlobHandle;
        
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var stateMachineBlob = stateMachinBlobHandle.Resolve();
            var clipsBlob = clipsBlobHandle.Resolve();

            var stateMachine = new AnimationStateMachine()
            {
                StateMachineBlob = stateMachineBlob,
                ClipsBlob = clipsBlob,
                CurrentState = AnimationState.Null,
                NextState = AnimationState.Null,
                CurrentTransition = StateTransition.Null
            };

            dstManager.AddComponentData(entity, stateMachine);
            dstManager.AddBuffer<ClipSampler>(entity);

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

            var ownerEntity = conversionSystem.GetPrimaryEntity(Owner);
            dstManager.AddComponentData(ownerEntity, new AnimatorOwner() { AnimatorEntity = entity });
            dstManager.AddComponentData(ownerEntity, new TransferRootMotion());
            dstManager.AddComponentData(entity, new AnimatorEntity() { Owner = ownerEntity});

            dstManager.AddComponentData(entity, new RootDeltaTranslation());
            dstManager.AddComponentData(entity, new RootDeltaRotation());
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager, GameObjectConversionSystem conversionSystem)
        {
            var clips = StateMachineAsset.Clips.Select(c => new SkeletonClipConfig()
            {
                clip = c.Clip,
                settings = SkeletonClipCompressionSettings.kDefaultSettings
            });
            clipsBlobHandle = conversionSystem.CreateBlob(Animator.gameObject, new SkeletonClipSetBakeData()
            {
                animator = Animator,
                clips = clips.ToArray()
            });

            stateMachinBlobHandle = conversionSystem.CreateBlob(Animator.gameObject, new StateMachineBlobBakeData()
            {
                StateMachineAsset = StateMachineAsset
            });
        }
    }
}