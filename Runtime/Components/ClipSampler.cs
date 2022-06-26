using Latios.Kinemation;
using Unity.Entities;

namespace DOTSAnimation
{
    public struct ClipSampler : IBufferElementData
    {
        public BlobAssetReference<SkeletonClipSetBlob> Blob;
        public int ClipIndex;
        public float Threshold;
        public float Time;
        public float Weight;
        public float Speed;
        
        public ref SkeletonClip Clip => ref Blob.Value.clips[ClipIndex];
    }
}