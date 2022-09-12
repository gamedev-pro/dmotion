using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Assertions;
using Unity.Entities;
using Unity.Entities.Exposed;

namespace DMotion
{
    public struct SkeletonClipHandle
    {
        public BlobAssetReference<SkeletonClipSetBlob> Clips;
        public ushort ClipIndex;
        public ref SkeletonClip Clip => ref Clips.Value.clips[ClipIndex];

        public SkeletonClipHandle(BlobAssetReference<SkeletonClipSetBlob> clips, int clipIndex)
        {
            Assert.IsTrue(clipIndex >= 0 && clipIndex < clips.Value.clips.Length);
            Clips = clips;
            ClipIndex = (ushort) clipIndex;
        }
    }
    
    internal struct ClipSampler : IBufferElementData, IElementWithId
    {
        public byte Id { get; set; }
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEventsBlob;
        internal ushort ClipIndex;
        internal float PreviousTime;
        internal float Time;
        internal float Weight;

        internal ref SkeletonClip Clip => ref Clips.Value.clips[ClipIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LoopToClipTime()
        {
            Time = Clip.LoopToClipTime(Time);
            PreviousTime = Clip.LoopToClipTime(PreviousTime);
        }
    }
}