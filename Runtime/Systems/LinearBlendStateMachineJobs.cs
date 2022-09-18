using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal partial struct UpdateLinearBlendStateMachineStatesJob : IJobEntityBatch
    {
        internal float DeltaTime;
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
                
                Execute(DeltaTime, ref clipSamplers, playables, linearBlendStates, blendParameters);
            }
        }

        internal static void Execute(
            float dt,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            in DynamicBuffer<PlayableState> playableStates,
            in DynamicBuffer<LinearBlendStateMachineState> linearBlendStates,
            in DynamicBuffer<BlendParameter> blendParameters
        )
        {
            for (var i = 0; i < linearBlendStates.Length; i++)
            {
                if (playableStates.TryGetWithId(linearBlendStates[i].PlayableId, out var playable))
                {
                    var linearBlendState = linearBlendStates[i];
                    LinearBlendStateUtils.ExtractLinearBlendVariablesFromStateMachine(linearBlendState,
                        blendParameters, out var blendRatio, out var thresholds);

                    LinearBlendStateUtils.UpdateSamplers(
                        dt,
                        blendRatio,
                        thresholds,
                        playable,
                        ref clipSamplers);
                }
            }
        }
    }

    [BurstCompile]
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
        
        internal static void Execute(
            ref DynamicBuffer<LinearBlendStateMachineState> linearBlendStates,
            in DynamicBuffer<PlayableState> playableStates
        )
        {
            for (int i = linearBlendStates.Length - 1; i >= 0; i--)
            {
                if (!playableStates.TryGetWithId(linearBlendStates[i].PlayableId, out _))
                {
                    linearBlendStates.RemoveAtSwapBack(i);
                }
            }
        }
    }
}