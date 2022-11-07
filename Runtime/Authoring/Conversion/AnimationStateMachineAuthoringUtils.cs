using System.Collections.Generic;
using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Latios.Kinemation.Authoring;
using UnityEngine;

namespace DMotion.Authoring
{
    public static class AnimationStateMachineAuthoringUtils
    {
        public static SmartBlobberHandle<SkeletonClipSetBlob> RequestClipsBlob(
            this GameObjectConversionSystem conversionSystem,
            Animator animator,
            params AnimationClipAsset[] clipAssets)
        {
            return conversionSystem.RequestClipsBlob(animator, (IEnumerable<AnimationClipAsset>) clipAssets);
        }
        
        public static SmartBlobberHandle<SkeletonClipSetBlob> RequestClipsBlob(
            this GameObjectConversionSystem conversionSystem,
            Animator animator,
            IEnumerable<AnimationClipAsset> clipAssets)
        {
            var clips = clipAssets.Select(c => new SkeletonClipConfig()
            {
                clip = c.Clip,
                settings = SkeletonClipCompressionSettings.kDefaultSettings
            });
            return conversionSystem.CreateBlob(animator.gameObject, new SkeletonClipSetBakeData()
            {
                animator = animator,
                clips = clips.ToArray()
            });
        }
        public static SmartBlobberHandle<StateMachineBlob> RequestStateMachineBlob(
            this GameObjectConversionSystem conversionSystem,
            GameObject gameObject,
            StateMachineBlobBakeData bakeData)
        {
            return conversionSystem.World.GetExistingSystem<AnimationStateMachineSmartBlobberSystem>()
                .AddToConvert(gameObject, bakeData);
        }
        
        public static SmartBlobberHandle<ClipEventsBlob> RequestClipEventsBlob(
            this GameObjectConversionSystem conversionSystem,
            GameObject gameObject,
            params AnimationClipAsset[] clips)
        {
            return conversionSystem.RequestClipEventsBlob(gameObject, (IEnumerable<AnimationClipAsset>)clips);
        }
        
        public static SmartBlobberHandle<ClipEventsBlob> RequestClipEventsBlob(
            this GameObjectConversionSystem conversionSystem,
            GameObject gameObject,
            IEnumerable<AnimationClipAsset> clips)
        {
            return conversionSystem.World.GetExistingSystem<ClipEventsSmartBlobberSystem>()
                .AddToConvert(gameObject, new ClipEventsBlobBakeData{Clips = clips.ToArray()});
        }
    }
}