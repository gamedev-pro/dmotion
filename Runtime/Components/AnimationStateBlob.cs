using Unity.Entities;

namespace DMotion
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
        internal BlobArray<StateOutTransitionGroup> Transitions;
    }
    
    internal struct SingleClipStateBlob
    {
        internal ushort ClipIndex;
    }

    internal struct LinearBlendStateBlob
    {
        internal BlobArray<int> SortedClipIndexes;
        internal BlobArray<float> SortedClipThresholds;
        internal ushort BlendParameterIndex;
    }
}