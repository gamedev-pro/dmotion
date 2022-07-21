using Latios.Authoring.Systems;
using Latios.Kinemation.Authoring.Systems;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public struct ClipEventsBlobBakeData
    {
        public AnimationClipAsset[] Clips;
    }
    
    internal struct ClipEventsConversionData
    {
        internal UnsafeList<DOTSAnimation.AnimationClipEvent> Events;
    }
    
    internal struct ClipEventsBlobConverter : ISmartBlobberSimpleBuilder<ClipEventsBlob>
    {
        internal UnsafeList<ClipEventsConversionData> ClipEvents;
        public unsafe BlobAssetReference<ClipEventsBlob> BuildBlob()
        {
            var builder = new BlobBuilder(Allocator.Temp);
            ref var root = ref builder.ConstructRoot<ClipEventsBlob>();
            var clipEvents = builder.Allocate(ref root.ClipEvents, ClipEvents.Length);
            for (var i = 0; i < clipEvents.Length; i++)
            {
                builder.ConstructFromNativeArray(ref clipEvents[i].Events, ClipEvents[i].Events.Ptr,
                    ClipEvents[i].Events.Length);
            }
            return builder.CreateBlobAssetReference<ClipEventsBlob>(Allocator.Persistent);
        }
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
                    Events = new UnsafeList<DOTSAnimation.AnimationClipEvent>(clipAssetEvents.Length, allocator)
                };
                clipEvents.Events.Resize(clipAssetEvents.Length);
                for (var eventIndex = 0; eventIndex < clipEvents.Events.Length; eventIndex++)
                {
                    var clipAssetEvent = clipAssetEvents[eventIndex];
                    clipEvents.Events[eventIndex] = new DOTSAnimation.AnimationClipEvent()
                    {
                        ClipIndex = (short) clipIndex,
                        EventHash = clipAssetEvent.Hash,
                        NormalizedTime = clipAssetEvent.NormalizedTime
                    };
                }

                converter.ClipEvents[clipIndex] = clipEvents;
            }

            return true;
        }
    }
}