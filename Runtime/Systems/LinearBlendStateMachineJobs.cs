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
            in DynamicBuffer<AnimationState> animationStates,
            in DynamicBuffer<LinearBlendStateMachineState> linearBlendStates,
            in DynamicBuffer<BlendParameter> blendParameters
        )
        {
            for (var i = 0; i < linearBlendStates.Length; i++)
            {
                if (animationStates.TryGetWithId(linearBlendStates[i].AnimationStateId, out var animationState))
                {
                    var linearBlendState = linearBlendStates[i];
                    LinearBlendStateUtils.ExtractLinearBlendVariablesFromStateMachine(linearBlendState,
                        blendParameters, out var blendRatio, out var thresholds, out var speeds);

                    LinearBlendStateUtils.UpdateSamplers(
                        DeltaTime,
                        blendRatio,
                        thresholds,
                        speeds,
                        animationState,
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
            in DynamicBuffer<AnimationState> animationStates
        )
        {
            for (int i = linearBlendStates.Length - 1; i >= 0; i--)
            {
                if (!animationStates.TryGetWithId(linearBlendStates[i].AnimationStateId, out _))
                {
                    linearBlendStates.RemoveAtSwapBack(i);
                }
            }
        }
    }
}