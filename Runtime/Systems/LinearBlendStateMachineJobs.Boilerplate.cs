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
        internal BufferTypeHandle<AnimationState> AnimationStatesHandle;
        [ReadOnly]
        internal BufferTypeHandle<BlendParameter> BlendParametersStateHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var clipSamplersAccessor = batchInChunk.GetBufferAccessor(ClipSamplersHandle);
            var linearBlendStatesAccessor = batchInChunk.GetBufferAccessor(LinearBlendStateMachineStatesHandle);
            var animationStatesAccessor = batchInChunk.GetBufferAccessor(AnimationStatesHandle);
            var blendParametersAccessor = batchInChunk.GetBufferAccessor(BlendParametersStateHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var clipSamplers = clipSamplersAccessor[i];
                var linearBlendStates = linearBlendStatesAccessor[i];
                var animationStates = animationStatesAccessor[i];
                var blendParameters = blendParametersAccessor[i];
                
                Execute(ref clipSamplers, animationStates, linearBlendStates, blendParameters);
            }
        }
    }

    internal partial struct CleanLinearBlendStatesJob : IJobEntityBatch
    {
        internal BufferTypeHandle<LinearBlendStateMachineState> LinearBlendStateMachineStates;
        [ReadOnly]
        internal BufferTypeHandle<AnimationState> AnimationStatesHandle;

        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            var linearBlendStatesAccessor = batchInChunk.GetBufferAccessor(LinearBlendStateMachineStates);
            var animationStatesAccessor = batchInChunk.GetBufferAccessor(AnimationStatesHandle);
            
            for(var i = 0; i < batchInChunk.Count; i++)
            {
                var linearBlendStates = linearBlendStatesAccessor[i];
                var animationStates = animationStatesAccessor[i];
                
                Execute(ref linearBlendStates, animationStates);
            }
        }
    }
}