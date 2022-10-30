using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Assertions;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(AnimationStateMachineSystem))]
    public partial class PlayOneShotSystem : SystemBase
    {
        [BurstCompile]
        internal partial struct PlayOneShotJob : IJobEntity
        {
            internal void Execute(
                ref AnimationStateTransitionRequest animationStateTransitionRequest,
                ref AnimationPreserveState animationPreserveState,
                ref PlayOneShotRequest playOneShot,
                ref OneShotState oneShotState,
                ref DynamicBuffer<SingleClipState> singleClipStates,
                ref DynamicBuffer<AnimationState> animationStates,
                ref DynamicBuffer<ClipSampler> clipSamplers,
                in AnimationCurrentState animationCurrentState
            )
            {
                //Evaluate requested one shot
                {
                    if (playOneShot.IsValid)
                    {
                        var singleClipAnimationState = SingleClipStateUtils.New(
                            (ushort)playOneShot.ClipIndex,
                            playOneShot.Speed,
                            false,
                            playOneShot.Clips,
                            playOneShot.ClipEvents,
                            ref singleClipStates,
                            ref animationStates,
                            ref clipSamplers);

                        animationStateTransitionRequest = AnimationStateTransitionRequest.New(
                            singleClipAnimationState.AnimationStateId,
                            playOneShot.TransitionDuration);

                        var animationState = animationStates.GetWithId(singleClipAnimationState.AnimationStateId);
                        var playOneShotClip = clipSamplers.GetWithId(animationState.StartSamplerId);

                        var endTime = playOneShot.EndTime * playOneShotClip.Clip.duration;
                        var blendOutDuration = playOneShot.TransitionDuration;

                        oneShotState = OneShotState.New(singleClipAnimationState.AnimationStateId, endTime,
                            blendOutDuration);
                        playOneShot = PlayOneShotRequest.Null;

                        if (!animationPreserveState.IsValid)
                        {
                            animationPreserveState.AnimationStateId = animationCurrentState.AnimationStateId;
                        }
                    }
                }

                //Evaluate one shot end
                {
                    if (oneShotState.IsValid && oneShotState.AnimationStateId == animationCurrentState.AnimationStateId)
                    {
                        var oneShotAnimationState = animationStates.GetWithId((byte)oneShotState.AnimationStateId);

                        //request transition back to state machine if we're done
                        if (oneShotAnimationState.Time >= oneShotState.EndTime)
                        {
                            Assert.IsTrue(animationPreserveState.IsValid,
                                "PlayOneShot: Preserve state is invalid while trying to return to previous animation state");
                            if (animationPreserveState.IsValid)
                            {
                                animationStateTransitionRequest = AnimationStateTransitionRequest.New(
                                    (byte)animationPreserveState.AnimationStateId, oneShotState.BlendOutDuration);
                                animationPreserveState = AnimationPreserveState.Null;
                            }

                            oneShotState = OneShotState.Null;
                        }
                    }
                }
            }
        }

        protected override void OnUpdate()
        {
            new PlayOneShotJob().ScheduleParallel();
        }
    }
}