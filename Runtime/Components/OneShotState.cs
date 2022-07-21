using Latios.Kinemation;
using Unity.Entities;

namespace DOTSAnimation
{
    public struct PlayOneShotRequest : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEvents;
        internal short ClipIndex;
        internal float NormalizedTransitionDuration;
        internal float Speed;

        public bool IsValid => ClipIndex >= 0 && Clips.IsCreated;

        public static PlayOneShotRequest Null => new PlayOneShotRequest() { ClipIndex = -1 };

        public PlayOneShotRequest(BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents, int clipIndex,
            float normalizedTransitionDuration = 0.15f,
            float speed = 1)
        {
            Clips = clips;
            ClipEvents = clipEvents;
            ClipIndex = (short) clipIndex;
            NormalizedTransitionDuration = normalizedTransitionDuration;
            Speed = speed;
        }
    }
    internal struct OneShotState : IComponentData
    {
        internal short SamplerIndex;
        internal float NormalizedTransitionDuration;
        internal float Speed;

        internal bool IsValid => SamplerIndex >= 0;
        internal static OneShotState Null => new OneShotState() { SamplerIndex = -1 };
    }
}