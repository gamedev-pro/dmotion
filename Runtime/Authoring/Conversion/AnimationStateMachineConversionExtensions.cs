using System.Collections.Generic;
using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public static class AnimationStateMachineConversionExtensions
    {
        public static SmartBlobberHandle<SkeletonClipSetBlob> RequestCreateBlobAsset(
            this IBaker baker,
            Animator animator,
            params AnimationClipAsset[] clipAssets)
        {
            return baker.RequestCreateBlobAsset(animator, (IEnumerable<AnimationClipAsset>)clipAssets);
        }

        public static SmartBlobberHandle<SkeletonClipSetBlob> RequestCreateBlobAsset(
            this IBaker baker,
            Animator animator,
            IEnumerable<AnimationClipAsset> clipAssets)
        {
            var clips = clipAssets.Select(c => new SkeletonClipConfig
            {
                clip = c.Clip,
                settings = SkeletonClipCompressionSettings.kDefaultSettings
            });
            return baker.RequestCreateBlobAsset(animator,
                new NativeArray<SkeletonClipConfig>(clips.ToArray(), Allocator.Temp));
        }

        public static SmartBlobberHandle<ClipEventsBlob> RequestCreateBlobAsset(
            this IBaker baker,
            params AnimationClipAsset[] clips)
        {
            return baker.RequestCreateBlobAsset<ClipEventsBlob, ClipEventSmartBlobberFilter>(
                new ClipEventSmartBlobberFilter()
                {
                    Clips = clips
                });
        }

        public static SmartBlobberHandle<ClipEventsBlob> RequestCreateBlobAsset(
            this IBaker baker,
            IEnumerable<AnimationClipAsset> clips)
        {
            return baker.RequestCreateBlobAsset(clips.ToArray());
        }

        public static SmartBlobberHandle<StateMachineBlob> RequestCreateBlobAsset(this IBaker baker,
            StateMachineAsset stateMachineAsset)
        {
            return baker.RequestCreateBlobAsset<StateMachineBlob, AnimationStateMachineSmartBlobberFilter>(
                new AnimationStateMachineSmartBlobberFilter
                {
                    StateMachine = stateMachineAsset
                });
        }
    }
}