using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(PlayablesSystem))]
    internal partial class SingleClipStateSystem : SystemBase
    {
        [BurstCompile]
        internal partial struct UpdateSingleClipStateMachineStatesJob : IJobEntity
        {
            internal float DeltaTime;
            internal void Execute(
                ref DynamicBuffer<SingleClipState> singleClipStates,
                ref DynamicBuffer<ClipSampler> clipSamplers,
                in DynamicBuffer<PlayableState> playableStates
            )
            {
                for (var i = 0; i < singleClipStates.Length; i++)
                {
                    if (playableStates.TryGetWithId(singleClipStates[i].PlayableId, out var playable))
                    {
                        singleClipStates[i]
                            .UpdateSamplers(DeltaTime, playable, ref clipSamplers);
                    }
                }
            }
        }
        
        [BurstCompile]
        internal partial struct CleanSingleClipStatesJob : IJobEntity
        {
            internal void Execute(
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

        protected override void OnUpdate()
        {
            new UpdateSingleClipStateMachineStatesJob
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel();

            new CleanSingleClipStatesJob().ScheduleParallel();
        }
    }
}