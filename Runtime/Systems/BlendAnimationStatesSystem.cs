using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(ClipSamplingSystem))]
    public partial class BlendAnimationStatesSystem : SystemBase
    {
        [BurstCompile]
        internal partial struct BlendAnimationStatesJob : IJobEntity
        {
            internal float DeltaTime;

            internal void Execute(
                ref AnimationStateTransition animationStateTransition,
                ref AnimationCurrentState animationCurrentState,
                ref AnimationStateTransitionRequest transitionRequest,
                ref DynamicBuffer<AnimationState> animationStates
            )
            {
                for (var i = 0; i < animationStates.Length; i++)
                {
                    var animationState = animationStates[i];
                    animationState.Time += DeltaTime * animationState.Speed;
                    animationStates[i] = animationState;
                }

                //Check for new transition
                if (transitionRequest.IsValid)
                {
                    var newToStateIndex = animationStates.IdToIndex((byte)transitionRequest.AnimationStateId);
                    //if we don't have a valid state, just transition instantly
                    var transitionDuration = animationCurrentState.IsValid ? transitionRequest.TransitionDuration : 0;
                    if (newToStateIndex >= 0)
                    {
                        animationStateTransition = new AnimationStateTransition
                        {
                            AnimationStateId = transitionRequest.AnimationStateId,
                            TransitionDuration = transitionDuration,
                        };
                    }

                    transitionRequest = AnimationStateTransitionRequest.Null;
                }

                var toAnimationStateIndex = animationStates.IdToIndex((byte)animationStateTransition.AnimationStateId);
                

                //Execute blend
                if (toAnimationStateIndex >= 0)
                {
                    //Check if the current transition has ended
                    if (animationStateTransition.HasEnded(animationStates[toAnimationStateIndex]))
                    {
                        animationCurrentState =
                            AnimationCurrentState.New(animationStateTransition.AnimationStateId);
                        animationStateTransition = AnimationStateTransition.Null;
                    }

                    var toAnimationState = animationStates[toAnimationStateIndex];

                    if (mathex.iszero(animationStateTransition.TransitionDuration))
                    {
                        toAnimationState.Weight = 1;
                    }
                    else
                    {
                        toAnimationState.Weight = math.clamp(toAnimationState.Time /
                                                             animationStateTransition.TransitionDuration, 0, 1);
                    }

                    animationStates[toAnimationStateIndex] = toAnimationState;

                    //We only blend if we have more than one state
                    if (animationStates.Length > 1)
                    {
                        //normalize weights
                        var sumWeights = 0.0f;
                        for (var i = 0; i < animationStates.Length; i++)
                        {
                            if (i != toAnimationStateIndex)
                            {
                                sumWeights += animationStates[i].Weight;
                            }
                        }

                        Assert.IsFalse(mathex.iszero(sumWeights),
                            "Remaining weights are zero. Did AnimationStates not get cleaned up?");

                        var targetWeight = 1 - toAnimationState.Weight;
                        var inverseSumWeights = targetWeight / sumWeights;
                        for (var i = 0; i < animationStates.Length; i++)
                        {
                            if (i != toAnimationStateIndex)
                            {
                                var animationState = animationStates[i];
                                animationState.Weight *= inverseSumWeights;
                                animationStates[i] = animationState;
                            }
                        }
                    }
                }
            }
        }

        [BurstCompile]
        internal partial struct CleanAnimationStatesJob : IJobEntity
        {
            internal float DeltaTime;

            internal void Execute(
                in AnimationStateTransition transition,
                ref DynamicBuffer<AnimationState> animationStates,
                ref DynamicBuffer<ClipSampler> samplers)
            {
                //After all transitions are handled, clean up animationState states with zero Weights
                var toAnimationStateIndex = animationStates.IdToIndex((byte)transition.AnimationStateId);
                for (var i = animationStates.Length - 1; i >= 0; i--)
                {
                    if (i != toAnimationStateIndex)
                    {
                        var animationState = animationStates[i];
                        if (mathex.iszero(animationState.Weight))
                        {
                            //TODO (perf): Could we improve performance by batching all removes? (we may need to pay for sorting at the end)
                            var removeCount = animationState.ClipCount;
                            Assert.IsTrue(removeCount > 0,
                                "AnimationState doesn't declare clip count to remove. This will lead to sampler leak");
                            samplers.RemoveRangeWithId(animationState.StartSamplerId, removeCount);
                            animationStates.RemoveAt(i);
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            new BlendAnimationStatesJob
            {
                DeltaTime = Time.DeltaTime
            }.ScheduleParallel();
            new CleanAnimationStatesJob().ScheduleParallel();
        }
    }
}