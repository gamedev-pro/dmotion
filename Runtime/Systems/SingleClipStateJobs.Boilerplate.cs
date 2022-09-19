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
        internal BufferTypeHandle<AnimationState> AnimationStatesHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var clipSamplersAccessor = batchInChunk.GetBufferAccessor(ClipSamplersHandle);
            var singleStatesAccessor = batchInChunk.GetBufferAccessor(SingleClipStatesHandle);
            var animationStatesAccessor = batchInChunk.GetBufferAccessor(AnimationStatesHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var clipSamplers = clipSamplersAccessor[i];
                var singleStates = singleStatesAccessor[i];
                var animationStates = animationStatesAccessor[i];
                
                Execute(ref clipSamplers, singleStates, animationStates);
            }
        }
    }
    
    internal partial struct CleanSingleClipStatesJob : IJobEntityBatch
    {
        internal BufferTypeHandle<SingleClipState> SingleClipStatesHandle;
        [ReadOnly]
        internal BufferTypeHandle<AnimationState> AnimationStatesHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var singleStatesAccessor = batchInChunk.GetBufferAccessor(SingleClipStatesHandle);
            var animationStatesAccessor = batchInChunk.GetBufferAccessor(AnimationStatesHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var singleStates = singleStatesAccessor[i];
                var animationStates = animationStatesAccessor[i];
                
                Execute(ref singleStates, animationStates);
            }
        }
    }
}