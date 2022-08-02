using Latios.Authoring.Systems;
using Latios.Kinemation.Authoring.Systems;
using Unity.Collections.LowLevel.Unsafe;
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
            converter = new ClipEventsBlobConverter();
            var allocator = World.UpdateAllocator.ToAllocator;
            converter.ClipEvents = new UnsafeList<ClipEventsConversionData>(input.Clips.Length, allocator);
            converter.ClipEvents.Resize(input.Clips.Length);
            for (var clipIndex = 0; clipIndex < converter.ClipEvents.Length; clipIndex++)
            {
                var clipAssetEvents = input.Clips[clipIndex].Events;
                var clipEvents = new ClipEventsConversionData
                {
                    Events = new UnsafeList<DMotion.AnimationClipEvent>(clipAssetEvents.Length, allocator)
                };
                clipEvents.Events.Resize(clipAssetEvents.Length);
                for (var eventIndex = 0; eventIndex < clipEvents.Events.Length; eventIndex++)
                {
                    var clipAssetEvent = clipAssetEvents[eventIndex];
                    clipEvents.Events[eventIndex] = new DMotion.AnimationClipEvent()
                    {
                        ClipIndex = (short) clipIndex,
                        EventHash = clipAssetEvent.Hash,
                        ClipTime = clipAssetEvent.Time
                    };
                }

                converter.ClipEvents[clipIndex] = clipEvents;
            }

            return true;
        }
    }
}