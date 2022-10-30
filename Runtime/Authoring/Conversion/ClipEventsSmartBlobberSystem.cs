using Latios.Authoring.Systems;
using Latios.Kinemation.Authoring.Systems;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public struct ClipEventsBlobBakeData
    {
        public AnimationClipAsset[] Clips;
    }

    [UpdateAfter(typeof(SkeletonClipSetSmartBlobberSystem))]
    internal class ClipEventsSmartBlobberSystem : SmartBlobberConversionSystem<ClipEventsBlob,
        ClipEventsBlobBakeData, ClipEventsBlobConverter>
    {
        protected override bool Filter(in ClipEventsBlobBakeData input, GameObject gameObject, out ClipEventsBlobConverter converter)
        {
            converter = ClipEventsAuthoringUtils.CreateConverter(input.Clips, World.UpdateAllocator.ToAllocator);
            return true;
        }
    }
}