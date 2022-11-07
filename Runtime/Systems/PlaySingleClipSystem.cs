using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(AnimationStateMachineSystem))]
    public partial class PlaySingleClipSystem : SystemBase
    {
        [BurstCompile]
        internal partial struct PlaySingleClipJob : IJobEntity
        {
            internal void Execute(
                ref AnimationStateTransitionRequest animationStateTransitionRequest,
                ref PlaySingleClipRequest playSingleClipRequest,
                ref DynamicBuffer<SingleClipState> singleClipStates,
                ref DynamicBuffer<AnimationState> animationStates,
                ref DynamicBuffer<ClipSampler> clipSamplers
            )
            {
                if (playSingleClipRequest.IsValid)
                {
                    var singleClipAnimationState = SingleClipStateUtils.New(
                        (ushort)playSingleClipRequest.ClipIndex,
                        playSingleClipRequest.Speed,
                        playSingleClipRequest.Loop,
                        playSingleClipRequest.Clips,
                        playSingleClipRequest.ClipEvents,
                        ref singleClipStates,
                        ref animationStates,
                        ref clipSamplers);

                    animationStateTransitionRequest = AnimationStateTransitionRequest.New(
                        singleClipAnimationState.AnimationStateId,
                        playSingleClipRequest.TransitionDuration);

                    playSingleClipRequest = PlaySingleClipRequest.Null;
                }
            }
        }

        protected override void OnUpdate()
        {
            new PlaySingleClipJob().ScheduleParallel();
        }
    }
}