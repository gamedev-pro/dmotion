using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    internal interface IElementWithId
    {
        byte Id { get; set; }
    }

    internal struct PlayableTransition : IComponentData
    {
        internal sbyte PlayableId;
        internal float TransitionDuration;
        internal float TransitionStartTime;
        internal bool IsValid => PlayableId >= 0;
        internal static PlayableTransition Null => new PlayableTransition() { PlayableId = -1 };
    }
    
    internal struct PlayableTransitionRequest : IComponentData
    {
        internal sbyte PlayableId;
        internal float TransitionDuration;
        internal bool IsValid => PlayableId >= 0;

        internal static PlayableTransitionRequest Null => new PlayableTransitionRequest() { PlayableId = -1 };
    }
    
    [BurstCompile]
    internal struct PlayableState : IBufferElementData, IElementWithId
    {
        public byte Id { get; set; }
        internal float Time;
        internal float Weight;
        internal float Speed;
        internal bool Loop;
        internal byte StartSamplerId;
        internal byte ClipCount;

        internal static int New(ref DynamicBuffer<PlayableState> playableStates, byte clipCount, float speed, bool loop)
        {
            playableStates.AddWithId(new PlayableState
            {
                Speed = speed,
                Loop = loop,
                ClipCount = clipCount,
            }, out _, out var index);
            return index;
        }
    }
}