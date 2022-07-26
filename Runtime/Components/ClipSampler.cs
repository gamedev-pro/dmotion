using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Entities;

namespace DMotion
{
    internal struct ClipSampler : IBufferElementData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEventsBlob;
        internal ushort ClipIndex;
        internal float PreviousNormalizedTime;
        internal float NormalizedTime;
        internal float Weight;

        internal ref SkeletonClip Clip => ref Clips.Value.clips[ClipIndex];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void LoopToClipTime()
        {
            NormalizedTime = Clip.LoopToClipTime(NormalizedTime);
            PreviousNormalizedTime = Clip.LoopToClipTime(PreviousNormalizedTime);
        }
    }
}