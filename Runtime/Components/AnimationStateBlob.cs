using Unity.Entities;

namespace DOTSAnimation
{
    public enum StateType : byte
    {
        Single = 0,
        LinearBlend
    }

    internal struct AnimationStateBlob
    {
        internal StateType Type;
        internal ushort StateIndex;
        internal bool Loop;
        internal float Speed;
    }
    
    internal struct SingleClipStateBlob
    {
        internal ushort ClipIndex;
    }

    internal struct ClipWithThreshold
    {
        internal ushort ClipIndex;
        internal float Threshold;
    }
    internal struct LinearBlendStateBlob
    {
        internal BlobArray<ClipWithThreshold> ClipSortedByThreshold;
        internal ushort BlendParameterIndex;
    }
}