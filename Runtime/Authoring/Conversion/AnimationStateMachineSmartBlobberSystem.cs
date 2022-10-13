using Latios.Authoring.Systems;
using Latios.Kinemation.Authoring.Systems;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public struct StateMachineBlobBakeData
    {
        internal StateMachineAsset StateMachineAsset;
    }

    [UpdateAfter(typeof(SkeletonClipSetSmartBlobberSystem))]
    internal class AnimationStateMachineSmartBlobberSystem : SmartBlobberConversionSystem<StateMachineBlob,
        StateMachineBlobBakeData, StateMachineBlobConverter>
    {
        protected override bool Filter(in StateMachineBlobBakeData input, GameObject gameObject,
            out StateMachineBlobConverter converter)
        {
            var allocator = World.UpdateAllocator.ToAllocator;
            converter = AnimationStateMachineConversionUtils.CreateConverter(input.StateMachineAsset, allocator);

            return true;
        }
    }
}