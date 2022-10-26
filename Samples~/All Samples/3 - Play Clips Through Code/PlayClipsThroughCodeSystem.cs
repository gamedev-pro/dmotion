using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.PlayClipsThroughCode
{
    public struct PlayClipsThroughCodeComponent : IComponentData
    {
        public SingleClipRef WalkClip;
        public SingleClipRef RunClip;
        public float TransitionDuration;
    }

    [DisableAutoCreation]
    public partial class PlayClipsThroughCodeSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var playWalk = Input.GetKeyDown(KeyCode.Alpha1);
            var playRun = Input.GetKeyDown(KeyCode.Alpha2);

            Entities.ForEach((ref AnimationStateTransitionRequest transitionRequest,
                ref DynamicBuffer<SingleClipState> singleClips,
                ref DynamicBuffer<AnimationState> animationStates, ref DynamicBuffer<ClipSampler> clipSamplers,
                in AnimationCurrentState animationCurrentState,
                in PlayClipsThroughCodeComponent playClipsComponent) =>
            {
                if (playWalk && !playClipsComponent.WalkClip.IsPlayingOrTransitioningTo(transitionRequest,
                        animationCurrentState, animationStates, clipSamplers))
                {
                    playClipsComponent.WalkClip.PlaySingleClip(ref transitionRequest, ref singleClips,
                        ref animationStates, ref clipSamplers, playClipsComponent.TransitionDuration);
                }
                else if (playRun && !playClipsComponent.RunClip.IsPlayingOrTransitioningTo(transitionRequest,
                             animationCurrentState, animationStates, clipSamplers))
                {
                    playClipsComponent.RunClip.PlaySingleClip(ref transitionRequest, ref singleClips,
                        ref animationStates, ref clipSamplers, playClipsComponent.TransitionDuration);
                }
            }).Schedule();
        }
    }
}