using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion
{
    internal partial struct UpdateSingleClipStatesJob : IJobEntityBatch
    {
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
                
                Execute(ref clipSamplers, singleStates, playables);
            }
        }
    }
    
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
    }
}