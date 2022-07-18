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
        public Animator Animator;
        public AnimationStateMachineAsset StateMachineAsset;
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

            var boolParameters = dstManager.GetOrCreateBuffer<BoolParameter>(entity);
            for (ushort i = 0; i < stateMachineBlob.Value.Parameters.Length; i++)
            {
                boolParameters.Add(new BoolParameter()
                {
                    Hash = stateMachineBlob.Value.Parameters[i].Hash,
                    Value = false
                });
            }
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