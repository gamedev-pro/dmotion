using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal partial struct UpdateSingleClipStatesJob : IJobEntityBatch
    {
        internal float DeltaTime;
        [NativeDisableContainerSafetyRestriction]
        internal BufferTypeHandle<ClipSampler> ClipSamplersHandle;
        
        [ReadOnly]
        internal BufferTypeHandle<SingleClipState> SingleClipStatesHandle;
        [ReadOnly]
        internal BufferTypeHandle<PlayableState> PlayableStatesHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var clipSamplersAccessor = batchInChunk.GetBufferAccessor(ClipSamplersHandle);
            var singleStatesAccessor = batchInChunk.GetBufferAccessor(SingleClipStatesHandle);
            var playableStatesAccessor = batchInChunk.GetBufferAccessor(PlayableStatesHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var clipSamplers = clipSamplersAccessor[i];
                var singleStates = singleStatesAccessor[i];
                var playables = playableStatesAccessor[i];
                
                Execute(DeltaTime, ref clipSamplers, singleStates, playables);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Execute(float dt,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            in DynamicBuffer<SingleClipState> singleClipStates,
            in DynamicBuffer<PlayableState> playableStates
        )
        {
            for (var i = 0; i < singleClipStates.Length; i++)
            {
                if (playableStates.TryGetWithId(singleClipStates[i].PlayableId, out var playable))
                {
                    SingleClipStateUtils
                        .UpdateSamplers(singleClipStates[i], dt, playable, ref clipSamplers);
                }
            }
        }
    }
    
    [BurstCompile]
    internal partial struct CleanSingleClipStatesJob : IJobEntityBatch
    {
        internal BufferTypeHandle<SingleClipState> SingleClipStatesHandle;
        [ReadOnly]
        internal BufferTypeHandle<PlayableState> PlayableStatesHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var singleStatesAccessor = batchInChunk.GetBufferAccessor(SingleClipStatesHandle);
            var playableStatesAccessor = batchInChunk.GetBufferAccessor(PlayableStatesHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var singleStates = singleStatesAccessor[i];
                var playables = playableStatesAccessor[i];
                
                Execute(ref singleStates, playables);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Execute(
            ref DynamicBuffer<SingleClipState> singleClipStates,
            in DynamicBuffer<PlayableState> playableStates
        )
        {
            for (int i = singleClipStates.Length - 1; i >= 0; i--)
            {
                if (!playableStates.TryGetWithId(singleClipStates[i].PlayableId, out _))
                {
                    singleClipStates.RemoveAtSwapBack(i);
                }
            }
        }
    }
}