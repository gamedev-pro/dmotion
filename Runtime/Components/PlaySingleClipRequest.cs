using Latios.Kinemation;
using Unity.Entities;

namespace DMotion
{
    public struct PlaySingleClipRequest : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> Clips;
        internal BlobAssetReference<ClipEventsBlob> ClipEvents;
        internal short ClipIndex;
        internal float TransitionDuration;
        internal float Speed;
        internal bool Loop;

        public bool IsValid => ClipIndex >= 0 && Clips.IsCreated;

        public static PlaySingleClipRequest Null => new PlaySingleClipRequest() { ClipIndex = -1 };

        public static PlaySingleClipRequest New(in SingleClipRef singleClipRef, bool loop = true,
            float transitionDuration = 0.15f)
        {
            return New(singleClipRef.Clips, singleClipRef.ClipEvents, singleClipRef.ClipIndex, transitionDuration,
                singleClipRef.Speed,
                loop);
        }

        public static PlaySingleClipRequest New(BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents, int clipIndex,
            float transitionDuration = 0.15f,
            float speed = 1,
            bool loop = true)
        {
            return new PlaySingleClipRequest(clips, clipEvents, clipIndex, transitionDuration, speed, loop);
        }

        public PlaySingleClipRequest(BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents, int clipIndex,
            float transitionDuration = 0.15f,
            float speed = 1,
            bool loop = true)
        {
            Clips = clips;
            ClipEvents = clipEvents;
            ClipIndex = (short)clipIndex;
            TransitionDuration = transitionDuration;
            Speed = speed;
            Loop = loop;
        }
    }
}