using Latios.Kinemation;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace DMotion.Tests
{
    internal static class AnimationStateTestUtils
    {
        internal static SingleClipState CreateSingleClipState(EntityManager manager, Entity entity,
            float speed = 1.0f,
            bool loop = false,
            ushort clipIndex = 0)
        {
            var singleClips = manager.GetBuffer<SingleClipState>(entity);
            var playableStates = manager.GetBuffer<PlayableState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);

            var clipsBlob = CreateFakeSkeletonClipSetBlob(1);

            return SingleClipStateUtils.New(
                clipIndex, speed, loop,
                clipsBlob,
                BlobAssetReference<ClipEventsBlob>.Null,
                ref singleClips,
                ref playableStates,
                ref samplers
            );
        }

        internal static BlobAssetReference<SkeletonClipSetBlob> CreateFakeSkeletonClipSetBlob(int clipCount)
        {
            Assert.Greater(clipCount, 0);
            var     builder = new BlobBuilder(Allocator.Temp);
            ref var root    = ref builder.ConstructRoot<SkeletonClipSetBlob>();
            root.boneCount = 1;
            var blobClips   = builder.Allocate(ref root.clips, clipCount);
            for (int i = 0; i < clipCount; i++)
            {
                blobClips[i] = new SkeletonClip()
                {
                    duration = 1,
                    sampleRate = 1,
                    boneCount = 1,
                    name = $"Dummy Clip {i}"
                };
            }
            
            return builder.CreateBlobAssetReference<SkeletonClipSetBlob>(Allocator.Temp);
        }
    }
}