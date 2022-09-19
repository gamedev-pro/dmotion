using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

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
                ref AnimationStateMachineTransitionRequest stateMachineTransitionRequest,
                ref AnimationStateTransitionRequest animationStateTransitionRequest,
                ref PlayOneShotRequest playOneShot,
                ref OneShotState oneShotState,
                ref DynamicBuffer<SingleClipState> singleClipStates,
                ref DynamicBuffer<AnimationState> animationStates,
                ref DynamicBuffer<ClipSampler> clipSamplers,
                in AnimationCurrentState animationTransition
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

                        animationStateTransitionRequest = AnimationStateTransitionRequest.New(singleClipAnimationState.AnimationStateId,
                            playOneShot.TransitionDuration);

                        var animationState = animationStates.GetWithId(singleClipAnimationState.AnimationStateId);
                        var playOneShotClip = clipSamplers.GetWithId(animationState.StartSamplerId);

                        var endTime = playOneShot.EndTime * playOneShotClip.Clip.duration;
                        var blendOutDuration = playOneShot.TransitionDuration;
                        oneShotState = OneShotState.New(singleClipAnimationState.AnimationStateId,endTime, blendOutDuration);

                        playOneShot = PlayOneShotRequest.Null;
                    }
                }

                //Evaluate one shot end
                {
                    if (oneShotState.IsValid && oneShotState.AnimationStateId == animationTransition.AnimationStateId)
                    {
                        var oneShotAnimationState = animationStates.GetWithId((byte)oneShotState.AnimationStateId);

                        //request transition back to state machine if we're done
                        if (oneShotAnimationState.Time >= oneShotState.EndTime)
                        {
                            stateMachineTransitionRequest = AnimationStateMachineTransitionRequest.New(oneShotState.BlendOutDuration);
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