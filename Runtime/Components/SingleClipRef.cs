using Latios.Kinemation;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    public struct SingleClipRef
    {
        // We keep SkeletonClipSetBlob (and SkeletonClip) internal to avoid direct dependences to Latios.Kinemation
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEvents;
        public ushort ClipIndex;
        public float Speed;

        public bool IsValid => Clips.IsCreated && ClipEvents.IsCreated && ClipIndex >= 0 &&
                               ClipIndex < Clips.Value.clips.Length && ClipIndex < ClipEvents.Value.ClipEvents.Length;

        // We keep SkeletonClipSetBlob (and SkeletonClip) internal to avoid direct dependences to Latios.Kinemation
        internal ref SkeletonClip Clip => ref Clips.Value.clips[ClipIndex];
        public ref ClipEvents Events => ref ClipEvents.Value.ClipEvents[ClipIndex];
        public float ClipDuration => Clip.duration;
        public FixedString128Bytes Name => Clip.name;
        public float SampleRate => Clip.sampleRate;
    }
}