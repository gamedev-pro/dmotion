using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Unity.Collections;
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
            stateMachine.CurrentState = stateMachine.CreateState(0);
            dstManager.AddComponentData(entity, stateMachine);

            var clipSamplers = dstManager.AddBuffer<ClipSampler>(entity);
            clipSamplers.Length = AnimationStateMachine.kMaxSamplerCount;
            dstManager.AddComponent<ActiveSamplersCount>(entity);

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
            dstManager.AddComponentData(entity, new AnimatorEntity() { Owner = ownerEntity});
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