using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    internal interface IElementWithId
    {
        byte Id { get; set; }
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

        internal static int New(ref DynamicBuffer<PlayableState> playableStates, float speed,
            bool loop)
        {
            playableStates.AddWithId(new PlayableState
            {
                Speed = speed,
                Loop = loop
            }, out _, out var index);
            return index;
        }

        internal static void DestroyStateWithId(ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> clipSamplers, byte playableId)
        {
            var playableIndex = playableStates.IdToIndex(playableId);
            DestroyState(ref playableStates, ref clipSamplers, playableIndex);
        }
        
        internal static void DestroyState(ref DynamicBuffer<PlayableState> playableStates, ref DynamicBuffer<ClipSampler> clipSamplers, int playableIndex)
        {
            var playable = playableStates[playableIndex];
            var removeCount = playable.ClipCount;
            clipSamplers.RemoveRangeWithId(playable.StartSamplerId, removeCount);
            playableStates.RemoveAt(playableIndex);
        }

        internal void UpdateTime(float dt)
        {
            Time += dt * Speed;
        }
    }
}