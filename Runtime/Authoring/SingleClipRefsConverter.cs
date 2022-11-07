using System;
using System.Collections.Generic;
using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using UnityEngine;

namespace DMotion.Authoring
{
    [Serializable]
    public struct SingleClipRefConvertData
    {
        public AnimationClipAsset Clip;
        public float Speed;
        public static SingleClipRefConvertData Default => new () { Clip = null, Speed = 1 };
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
}