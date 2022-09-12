using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Assertions;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(ClipSamplingSystem))]
    public partial class PlayablesSystem : SystemBase
    {
        [BurstCompile]
        internal partial struct UpdatePlayablesJob : IJobEntity
        {
            internal float DeltaTime;

            internal void Execute(
                ref PlayableTransition playableTransition,
                ref PlayableTransitionRequest transitionRequest,
                ref DynamicBuffer<PlayableState> playableStates
            )
            {
                for (var i = 0; i < playableStates.Length; i++)
                {
                    var playable = playableStates[i];
                    playable.Time += DeltaTime * playable.Speed;
                    playableStates[i] = playable;
                }

                //Check for new transition
                var toPlayableIndex = -1;
                if (transitionRequest.IsValid)
                {
                    toPlayableIndex = playableStates.IdToIndex((byte)transitionRequest.PlayableId);
                    if (toPlayableIndex >= 0)
                    {
                        playableTransition.PlayableId = transitionRequest.PlayableId;
                        playableTransition.TransitionDuration = transitionRequest.TransitionDuration;
                        playableTransition.TransitionStartTime = playableStates[toPlayableIndex].Time;
                    }

                    transitionRequest = PlayableTransitionRequest.Null;
                }
                else
                {
                    toPlayableIndex = playableStates.IdToIndex((byte)playableTransition.PlayableId);

                    //Check if the current transition has ended
                    if (toPlayableIndex >= 0)
                    {
                        var endTime = playableTransition.TransitionStartTime + playableTransition.TransitionDuration;
                        if (playableStates[toPlayableIndex].Time > endTime)
                        {
                            playableTransition = PlayableTransition.Null;
                        }
                    }
                }

                //Execute blend
                if (toPlayableIndex >= 0)
                {
                    var toPlayable = playableStates[toPlayableIndex];

                    if (mathex.iszero(playableTransition.TransitionDuration))
                    {
                        toPlayable.Weight = 1;
                    }
                    else
                    {
                        toPlayable.Weight = math.clamp((toPlayable.Time - playableTransition.TransitionStartTime) /
                                                       playableTransition.TransitionDuration, 0, 1);
                    }

                    playableStates[toPlayableIndex] = toPlayable;

                    //normalize weights
                    var sumWeights = 0.0f;
                    for (var i = 0; i < playableStates.Length; i++)
                    {
                        if (i != toPlayableIndex)
                        {
                            sumWeights += playableStates[i].Weight;
                        }
                    }

                    var targetWeight = 1 - toPlayable.Weight;
                    var inverseSumWeights = targetWeight / sumWeights;
                    for (var i = 0; i < playableStates.Length; i++)
                    {
                        if (i != toPlayableIndex)
                        {
                            var playable = playableStates[i];
                            playable.Weight *= inverseSumWeights;
                            playableStates[i] = playable;
                        }
                    }
                }
            }
        }

        [BurstCompile]
        internal partial struct CleanPlayablesJob : IJobEntity
        {
            internal float DeltaTime;

            internal void Execute(
                in PlayableTransition transition,
                ref DynamicBuffer<PlayableState> playableStates,
                ref DynamicBuffer<ClipSampler> samplers)
            {
                //After all transitions are handled, clean up playable states with zero Weights
                var toPlayableIndex = playableStates.IdToIndex((byte)transition.PlayableId);
                for (var i = playableStates.Length - 1; i >= 0; i--)
                {
                    if (i != toPlayableIndex)
                    {
                        var playable = playableStates[i];
                        if (mathex.iszero(playable.Weight))
                        {
                            //TODO (perf): Could we improve performance by batching all removes? (we may need to pay for sorting at the end)
                            var removeCount = playable.ClipCount;
                            Assert.IsTrue(removeCount > 0, "Playable doesn't declare clip count to remove. This will lead to sampler leak");
                            samplers.RemoveRangeWithId(playable.StartSamplerId, removeCount);
                            playableStates.RemoveAt(i);
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            new UpdatePlayablesJob
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel();
            new CleanPlayablesJob().ScheduleParallel();
        }
    }
}