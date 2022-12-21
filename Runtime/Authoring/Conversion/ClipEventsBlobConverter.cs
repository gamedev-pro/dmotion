﻿using System;
using Latios.Authoring.Systems;
using Latios.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion.Authoring
{
    internal struct ClipEventsConversionData
    {
        internal UnsafeList<DMotion.AnimationClipEvent> Events;
    }
    
    [TemporaryBakingType]
    internal struct ClipEventsBlobConverter : IComponentData, IDisposable
    {
        internal UnsafeList<ClipEventsConversionData> ClipEvents;
        public readonly unsafe BlobAssetReference<ClipEventsBlob> BuildBlob()
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

        public void Dispose()
        {
            ClipEvents.Dispose();
        }
    }
}