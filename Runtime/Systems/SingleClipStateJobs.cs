using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal partial struct UpdateSingleClipStatesJob : IJobEntity
    {
        internal float DeltaTime;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Execute(
            ref DynamicBuffer<ClipSampler> clipSamplers,
            in DynamicBuffer<SingleClipState> singleClipStates,
            in DynamicBuffer<AnimationState> animationStates
        )
        {
            for (var i = 0; i < singleClipStates.Length; i++)
            {
                if (animationStates.TryGetWithId(singleClipStates[i].AnimationStateId, out var animationState))
                {
                    SingleClipStateUtils
                        .UpdateSamplers(singleClipStates[i], DeltaTime, animationState, ref clipSamplers);
                }
            }
        }
    }
    
    [BurstCompile]
    internal partial struct CleanSingleClipStatesJob : IJobEntity
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void Execute(
            ref DynamicBuffer<SingleClipState> singleClipStates,
            in DynamicBuffer<AnimationState> animationStates
        )
        {
            for (int i = singleClipStates.Length - 1; i >= 0; i--)
            {
                if (!animationStates.TryGetWithId(singleClipStates[i].AnimationStateId, out _))
                {
                    singleClipStates.RemoveAtSwapBack(i);
                }
            }
        }
    }
}