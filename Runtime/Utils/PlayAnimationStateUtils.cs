using Unity.Entities;

namespace DMotion
{
    public static class PlayAnimationStateUtils
    {
        public static void PlaySingleClip(this SingleClipRef singleClipRef,
            ref AnimationStateTransitionRequest animationStateTransitionRequest,
            ref DynamicBuffer<SingleClipState> singleClips, ref DynamicBuffer<AnimationState> animationStates,
            ref DynamicBuffer<ClipSampler> samplers, float transitionDuration = 0.15f)
        {
            var singleClipState = SingleClipStateUtils.New(singleClipRef.ClipIndex, singleClipRef.Speed,
                singleClipRef.Loop, singleClipRef.Clips, singleClipRef.ClipEvents, ref singleClips, ref animationStates,
                ref samplers);

            animationStateTransitionRequest.AnimationStateId = (sbyte) singleClipState.AnimationStateId;
            animationStateTransitionRequest.TransitionDuration = transitionDuration;
        }


        public static void PlaySingleClip(this SingleClipRef singleClipRef, EntityManager dstManager, Entity entity)
        {
            var singleClips = dstManager.GetBuffer<SingleClipState>(entity);
            var animationStates = dstManager.GetBuffer<AnimationState>(entity);
            var clipSamplers = dstManager.GetBuffer<ClipSampler>(entity);
            var transitionReq = dstManager.GetComponentData<AnimationStateTransitionRequest>(entity);
            
            singleClipRef.PlaySingleClip(ref transitionReq, ref singleClips, ref animationStates, ref clipSamplers, 0);
            dstManager.SetComponentData(entity, transitionReq);
        }
    }
}