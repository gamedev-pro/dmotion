using System;
using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Authoring
{
    public static class StateMachineEditorConstants
    {
        public const string DMotionPath = "DMotion";
    }

    public class AnimationStateMachineAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public GameObject Owner;
        public Animator Animator;
        public StateMachineAsset StateMachineAsset;

        public RootMotionMode RootMotionMode;
        public bool EnableEvents = true;

        private SmartBlobberHandle<SkeletonClipSetBlob> clipsBlobHandle;
        private SmartBlobberHandle<StateMachineBlob> stateMachineBlobHandle;
        private SmartBlobberHandle<ClipEventsBlob> clipEventsBlobHandle;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var stateMachineBlob = stateMachineBlobHandle.Resolve();
            var clipsBlob = clipsBlobHandle.Resolve();
            var clipEventsBlob = clipEventsBlobHandle.Resolve();

            AnimationStateMachineConversionUtils.AddStateMachineSystemComponents(dstManager, entity, StateMachineAsset,
                stateMachineBlob,
                clipsBlob,
                clipEventsBlob);
            AnimationStateMachineConversionUtils.AddAnimationStateSystemComponents(dstManager, entity);
            AnimationStateMachineConversionUtils.AddOneShotSystemComponents(dstManager, entity);
            
            if (EnableEvents && StateMachineAsset.Clips.Any(c => c.Events.Length > 0))
            {
                dstManager.GetOrCreateBuffer<RaisedAnimationEvent>(entity);
            }

            if (gameObject != Owner)
            {
                var ownerEntity = conversionSystem.GetPrimaryEntity(Owner);
                dstManager.AddComponentData(ownerEntity, new AnimatorOwner() { AnimatorEntity = entity });
                dstManager.AddComponentData(entity, new AnimatorEntity() { Owner = ownerEntity });
            }

            switch (RootMotionMode)
            {
                case RootMotionMode.Disabled:
                    break;
                case RootMotionMode.EnabledAutomatic:
                    dstManager.AddComponentData(entity, new RootDeltaTranslation());
                    dstManager.AddComponentData(entity, new RootDeltaRotation());
                    if (gameObject != Owner)
                    {
                        var ownerEntity = conversionSystem.GetPrimaryEntity(Owner);
                        dstManager.AddComponentData(ownerEntity, new TransferRootMotionToOwner());
                    }
                    else
                    {
                        dstManager.AddComponentData(entity, new ApplyRootMotionToEntity());
                    }

                    break;
                case RootMotionMode.EnabledManual:
                    dstManager.AddComponentData(entity, new RootDeltaTranslation());
                    dstManager.AddComponentData(entity, new RootDeltaRotation());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem)
        {
            ValidateStateMachine();
            clipsBlobHandle = conversionSystem.RequestClipsBlob(Animator, StateMachineAsset.Clips);
            stateMachineBlobHandle = conversionSystem.RequestStateMachineBlob(Animator.gameObject,
                new StateMachineBlobBakeData
                {
                    StateMachineAsset = StateMachineAsset
                });
            clipEventsBlobHandle = conversionSystem.RequestClipEventsBlob(Animator.gameObject, StateMachineAsset.Clips);
        }

        private void ValidateStateMachine()
        {
            foreach (var s in StateMachineAsset.States)
            {
                foreach (var c in s.Clips)
                {
                    Assert.IsTrue(c != null && c.Clip != null,
                        $"State ({s.name}) in State Machine {StateMachineAsset.name} has invalid clips");
                }
            }
        }
    }
}