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
                ref PlayableTransitionRequest playableTransitionRequest,
                ref PlayOneShotRequest playOneShot,
                ref OneShotState oneShotState,
                ref DynamicBuffer<SingleClipState> singleClipStates,
                ref DynamicBuffer<PlayableState> playableStates,
                ref DynamicBuffer<ClipSampler> clipSamplers,
                in PlayableCurrentState playableTransition
            )
            {
                //Evaluate requested one shot
                {
                    if (playOneShot.IsValid)
                    {
                        var singleClipPlayable = SingleClipStateUtils.New(
                            (ushort)playOneShot.ClipIndex,
                            playOneShot.Speed,
                            false,
                            playOneShot.Clips,
                            playOneShot.ClipEvents,
                            ref singleClipStates,
                            ref playableStates,
                            ref clipSamplers);

                        playableTransitionRequest = PlayableTransitionRequest.New(singleClipPlayable.PlayableId,
                            playOneShot.TransitionDuration);

                        var playableState = playableStates.GetWithId(singleClipPlayable.PlayableId);
                        var playOneShotClip = clipSamplers.GetWithId(playableState.StartSamplerId);

                        var endTime = playOneShot.EndTime * playOneShotClip.Clip.duration;
                        var blendOutDuration = playOneShot.TransitionDuration;
                        oneShotState = OneShotState.New(singleClipPlayable.PlayableId,endTime, blendOutDuration);

                        playOneShot = PlayOneShotRequest.Null;
                    }
                }

                //Evaluate one shot end
                {
                    if (oneShotState.IsValid && oneShotState.PlayableId == playableTransition.PlayableId)
                    {
                        var oneShotPlayableState = playableStates.GetWithId((byte)oneShotState.PlayableId);

                        //request transition back to state machine if we're done
                        if (oneShotPlayableState.Time >= oneShotState.EndTime)
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