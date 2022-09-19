using Latios.Kinemation;
using Unity.Entities;

namespace DMotion
{
    internal static class SingleClipStateUtils
    {
        public static SingleClipState NewForStateMachine(
            short stateIndex,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<SingleClipState> singleClips,
            ref DynamicBuffer<AnimationState> animationStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var state = ref stateMachineBlob.Value.States[stateIndex];
            var singleClipState = stateMachineBlob.Value.SingleClipStates[state.StateIndex];
            return New(singleClipState.ClipIndex, state.Speed, state.Loop,
                clips,
                clipEvents,
                ref singleClips,
                ref animationStates,
                ref samplers);
        }

        public static SingleClipState New(
            ushort clipIndex,
            float speed,
            bool loop,
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<SingleClipState> singleClips,
            ref DynamicBuffer<AnimationState> animationStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var singleClipState = new SingleClipState();

            var newSampler = new ClipSampler
            {
                ClipIndex = clipIndex,
                Clips = clips,
                ClipEventsBlob = clipEvents,
                PreviousTime = 0,
                Time = 0,
                Weight = 0
            };

            var animationStateIndex = AnimationState.New(ref animationStates, ref samplers, newSampler, speed, loop);
            singleClipState.AnimationStateId = animationStates[animationStateIndex].Id;
            singleClips.Add(singleClipState);
            return singleClipState;
        }

        public static void UpdateSamplers(SingleClipState singleClipState, float dt,
            in AnimationState animation,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var samplerIndex = samplers.IdToIndex(animation.StartSamplerId);
            var sampler = samplers[samplerIndex];
            sampler.Weight = animation.Weight;

            sampler.PreviousTime = sampler.Time;
            sampler.Time += dt * animation.Speed;
            if (animation.Loop)
            {
                sampler.LoopToClipTime();
            }

            samplers[samplerIndex] = sampler;
        }
    }
}