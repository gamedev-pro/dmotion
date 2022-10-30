﻿using Unity.Assertions;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public static class ClipEventsAuthoringUtils
    {
        public static BlobAssetReference<ClipEventsBlob> CreateClipEventsBlob(AnimationClipAsset[] clips,
            Allocator allocator)
        {
            return CreateConverter(clips, allocator).BuildBlob();
        }
        
        internal static ClipEventsBlobConverter CreateConverter(AnimationClipAsset[] clips,
            Allocator allocator)
        {
            var converter = new ClipEventsBlobConverter();
            converter.ClipEvents = new UnsafeList<ClipEventsConversionData>(clips.Length, allocator);
            converter.ClipEvents.Resize(clips.Length);
            for (var clipIndex = 0; clipIndex < converter.ClipEvents.Length; clipIndex++)
            {
                var clip = clips[clipIndex];
                Assert.IsNotNull(clip.Clip);
                var clipAssetEvents = clips[clipIndex].Events;
                var clipEvents = new ClipEventsConversionData
                {
                    Events = new UnsafeList<DMotion.AnimationClipEvent>(clipAssetEvents.Length, allocator)
                };
                clipEvents.Events.Resize(clipAssetEvents.Length);
                for (var eventIndex = 0; eventIndex < clipEvents.Events.Length; eventIndex++)
                {
                    var clipAssetEvent = clipAssetEvents[eventIndex];
                    clipEvents.Events[eventIndex] = new DMotion.AnimationClipEvent
                    {
                        ClipIndex = (short) clipIndex,
                        EventHash = clipAssetEvent.Hash,
                        ClipTime = Mathf.Clamp01(clipAssetEvent.NormalizedTime) * clip.Clip.length
                    };
                }

                converter.ClipEvents[clipIndex] = clipEvents;
            }

            return converter;
        }
    }
}