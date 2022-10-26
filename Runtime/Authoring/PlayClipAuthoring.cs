using System;
using System.Collections.Generic;
using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Authoring
{
    [Serializable]
    public struct SingleClipRefConvertData
    {
        public AnimationClipAsset Clip;
        public float Speed;
        public bool Loop;
    }

    public struct SingleClipRefsConverter
    {
        public Animator Animator;
        public SingleClipRefConvertData[] Clips;

        private SmartBlobberHandle<SkeletonClipSetBlob> clipsBlobHandle;
        private SmartBlobberHandle<ClipEventsBlob> clipEventsBlobHandle;

        public SingleClipRefsConverter(Animator animator, SingleClipRefConvertData[] clips)
        {
            Animator = animator;
            Clips = clips ?? Array.Empty<SingleClipRefConvertData>();
            clipsBlobHandle = default;
            clipEventsBlobHandle = default;
        }

        public IEnumerable<SingleClipRef> ConvertClips()
        {
            if (clipsBlobHandle.IsValid && clipEventsBlobHandle.IsValid)
            {
                var clipsBlob = clipsBlobHandle.Resolve();
                var clipEventsBlob = clipEventsBlobHandle.Resolve();

                for (int i = 0; i < clipsBlob.Value.clips.Length; i++)
                {
                    yield return new SingleClipRef
                    {
                        Clips = clipsBlob,
                        ClipEvents = clipEventsBlob,
                        ClipIndex = (ushort)i,
                        Speed = Clips[i].Speed,
                        Loop = Clips[i].Loop
                    };
                }
            }
        }

        public void RequestBlobAssets(GameObjectConversionSystem conversionSystem)
        {
            var validClips = Clips.Select(c => c.Clip).Where(c => c != null);
            clipsBlobHandle = conversionSystem.RequestClipsBlob(Animator, validClips);
            clipEventsBlobHandle =
                conversionSystem.RequestClipEventsBlob(Animator.gameObject, validClips);
        }
    }

    public class PlayClipAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
    {
        public GameObject Owner;
        public Animator Animator;
        public SingleClipRefConvertData Clip;

        public RootMotionMode RootMotionMode;
        public bool EnableEvents = true;

        private SingleClipRefsConverter singleClipsConverter;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var ownerEntity = gameObject != Owner ? conversionSystem.GetPrimaryEntity(Owner) : entity;
            AnimationStateMachineConversionUtils.AddSingleClipStateComponents(dstManager, ownerEntity, entity,
                EnableEvents, RootMotionMode);
            var singleClipRef = singleClipsConverter.ConvertClips().FirstOrDefault();
            if (singleClipRef.IsValid)
            {
                singleClipRef.PlaySingleClip(dstManager, entity);
            }
        }

        public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
            GameObjectConversionSystem conversionSystem)
        {
            Assert.IsNotNull(Clip.Clip, $"Trying to play null clip ({gameObject.name})");
            singleClipsConverter = new SingleClipRefsConverter(Animator, new[] { Clip });
            singleClipsConverter.RequestBlobAssets(conversionSystem);
        }
    }
}