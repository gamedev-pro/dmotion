using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion
{
    internal partial struct UpdateLinearBlendStateMachineStatesJob : IJobEntityBatch
    {
        [NativeDisableContainerSafetyRestriction]
        internal BufferTypeHandle<ClipSampler> ClipSamplersHandle;
        [ReadOnly]
        internal BufferTypeHandle<LinearBlendStateMachineState> LinearBlendStateMachineStatesHandle;
        [ReadOnly]
        internal BufferTypeHandle<PlayableState> PlayableStatesHandle;
        [ReadOnly]
        internal BufferTypeHandle<BlendParameter> BlendParametersStateHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var clipSamplersAccessor = batchInChunk.GetBufferAccessor(ClipSamplersHandle);
            var linearBlendStatesAccessor = batchInChunk.GetBufferAccessor(LinearBlendStateMachineStatesHandle);
            var playableStatesAccessor = batchInChunk.GetBufferAccessor(PlayableStatesHandle);
            var blendParametersAccessor = batchInChunk.GetBufferAccessor(BlendParametersStateHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var clipSamplers = clipSamplersAccessor[i];
                var linearBlendStates = linearBlendStatesAccessor[i];
                var playables = playableStatesAccessor[i];
                var blendParameters = blendParametersAccessor[i];
                
                Execute(ref clipSamplers, playables, linearBlendStates, blendParameters);
            }
        }
    }

    internal partial struct CleanLinearBlendStatesJob : IJobEntityBatch
    {
        internal BufferTypeHandle<LinearBlendStateMachineState> LinearBlendStateMachineStates;
        [ReadOnly]
        internal BufferTypeHandle<PlayableState> PlayableStatesHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var linearBlendStatesAccessor = batchInChunk.GetBufferAccessor(LinearBlendStateMachineStates);
            var playableStatesAccessor = batchInChunk.GetBufferAccessor(PlayableStatesHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var linearBlendStates = linearBlendStatesAccessor[i];
                var playables = playableStatesAccessor[i];
                
                Execute(ref linearBlendStates, playables);
            }
        }
    }
}