using Latios.Kinemation;
using Unity.Assertions;
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

            animationStateTransitionRequest.AnimationStateId = (sbyte)singleClipState.AnimationStateId;
            animationStateTransitionRequest.TransitionDuration = transitionDuration;
        }

        public static void PlaySingleClipOneShot(this SingleClipRef singleClipRef,
            ref PlayOneShotRequest playOneShotRequest,
            float transitionDuration = 0.15f, float normalizedEndTime = 0.8f)
        {
            Assert.IsTrue(normalizedEndTime is >= 0 and <= 1, "Normalized End Time must be within 0 and 1");
            
            playOneShotRequest = PlayOneShotRequest.New(singleClipRef.Clips, singleClipRef.ClipEvents,
                singleClipRef.ClipIndex, transitionDuration, normalizedEndTime, singleClipRef.Speed);
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

        public static bool IsPlayingOrTransitioningTo(this SingleClipRef singleClipRef,
            in AnimationStateTransitionRequest animationStateTransitionRequest,
            in AnimationCurrentState animationCurrentState,
            in DynamicBuffer<AnimationState> animationStates,
            in DynamicBuffer<ClipSampler> clipSamplers)
        {
            ref var clip = ref singleClipRef.Clip;
            if (animationCurrentState.IsValid)
            {
                if (IsPlaying(ref clip, (byte)animationCurrentState.AnimationStateId, animationStates, clipSamplers))
                {
                    return true;
                }
            }

            if (animationStateTransitionRequest.IsValid)
            {
                if (IsPlaying(ref clip, (byte)animationStateTransitionRequest.AnimationStateId, animationStates,
                        clipSamplers))
                {
                    return true;
                }
            }

            return false;
        }


        internal static bool IsPlaying(ref SkeletonClip clip,
            byte stateId,
            in DynamicBuffer<AnimationState> animationStates,
            in DynamicBuffer<ClipSampler> clipSamplers)
        {
            var currentState = animationStates.GetWithId(stateId);
            var startSamplerIndex = clipSamplers.IdToIndex(currentState.StartSamplerId);
            for (int i = startSamplerIndex; i < startSamplerIndex + currentState.ClipCount; i++)
            {
                var sampler = clipSamplers[i];
                if (SkeletonClipEquals(ref sampler.Clip, ref clip))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool SkeletonClipEquals(ref SkeletonClip a, ref SkeletonClip b)
        {
            return a.name == b.name && mathex.approximately(a.duration, b.duration) &&
                   mathex.approximately(a.sampleRate, b.sampleRate) &&
                   a.events.names.Length == b.events.names.Length;
        }
    }
}