using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.Assertions;

namespace DMotion
{
    internal interface IElementWithId
    {
        byte Id { get; set; }
    }

    internal struct PlayableCurrentState : IComponentData
    {
        internal sbyte PlayableId;
        internal bool IsValid => PlayableId >= 0;
        internal static PlayableCurrentState Null => new () { PlayableId = -1 };
        internal static PlayableCurrentState New(sbyte playableId)
        {
            return new PlayableCurrentState
            {
                PlayableId = playableId
            };
        }
    }
    
    internal struct PlayableTransition : IComponentData
    {
        internal sbyte PlayableId;
        internal float TransitionDuration;
        internal float TransitionStartTime;
        internal readonly float TransitionEndTime => TransitionStartTime + TransitionDuration;
        internal static PlayableTransition Null => new () { PlayableId = -1 };

        internal readonly bool HasEnded(in PlayableState playableState)
        {
            Assert.AreEqual(playableState.Id, PlayableId);
            return playableState.Time > TransitionEndTime;
        }
    }

    internal struct PlayableTransitionRequest : IComponentData
    {
        internal sbyte PlayableId;
        internal float TransitionDuration;
        internal bool IsValid => PlayableId >= 0;

        internal static PlayableTransitionRequest Null => new PlayableTransitionRequest() { PlayableId = -1 };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static PlayableTransitionRequest New(byte playableId, float transitionDuration)
        {
            return new PlayableTransitionRequest
            {
                PlayableId = (sbyte)playableId,
                TransitionDuration = transitionDuration
            };
        }
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

        internal static int New(
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers,
            NativeArray<ClipSampler> newSamplers,
            float speed, bool loop)
        {
            Assert.IsTrue(newSamplers.IsCreated);
            Assert.IsTrue(newSamplers.Length > 0);

            var clipCount = (byte)newSamplers.Length;
            playableStates.AddWithId(new PlayableState
            {
                Speed = speed,
                Loop = loop,
                ClipCount = clipCount
            }, out _, out var playableIndex);

            var playable = playableStates[playableIndex];
            if (samplers.TryFindIdAndInsertIndex(clipCount, out var id,
                    out var insertIndex))
            {
                samplers.Length += clipCount;
                playable.StartSamplerId = id;
                for (var i = 0; i < clipCount; i++)
                {
                    var sampler = newSamplers[i];
                    sampler.Id = (byte)(playable.StartSamplerId + i);

                    var samplerIndex = i + insertIndex;
                    samplers[samplerIndex] = sampler;
                }

                playableStates[playableIndex] = playable;
            }

            return playableIndex;
        }
    }
}