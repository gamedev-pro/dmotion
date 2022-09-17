using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PlayablesSystem))]
    internal partial class LinearBlendStateMachineSystem : SystemBase
    {
        [BurstCompile]
        internal partial struct UpdateLinearBlendStateMachineStatesJob : IJobEntity
        {
            internal float DeltaTime;

            internal void Execute(
                ref DynamicBuffer<LinearBlendStateMachineState> linearBlendStates,
                ref DynamicBuffer<ClipSampler> clipSamplers,
                in DynamicBuffer<PlayableState> playableStates,
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
        internal partial struct CleanLinearBlendStatesJob : IJobEntity
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

        protected override void OnUpdate()
        {
            new UpdateLinearBlendStateMachineStatesJob()
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel();

            new CleanLinearBlendStatesJob().ScheduleParallel();
        }
    }
}