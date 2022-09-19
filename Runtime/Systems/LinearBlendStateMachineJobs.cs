using Unity.Burst;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal partial struct UpdateLinearBlendStateMachineStatesJob
    {
        internal float DeltaTime;

        internal void Execute(
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
                        DeltaTime,
                        blendRatio,
                        thresholds,
                        playable,
                        ref clipSamplers);
                }
            }
        }
    }

    [BurstCompile]
    internal partial struct CleanLinearBlendStatesJob
    {
        internal void Execute(
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