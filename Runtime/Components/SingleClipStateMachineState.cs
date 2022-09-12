using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal struct SingleClipStateMachineState : IBufferElementData
    {
        internal byte PlayableId;

        public static SingleClipStateMachineState NewForStateMachine(
            short stateIndex,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<SingleClipStateMachineState> singleClips,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var state = ref stateMachineBlob.Value.States[stateIndex];
            var singleClipState = stateMachineBlob.Value.SingleClipStates[state.StateIndex];
            return New(singleClipState.ClipIndex, state.Speed, state.Loop,
                clips,
                clipEvents,
                ref singleClips,
                ref playableStates,
                ref samplers);
        }

        public static SingleClipStateMachineState New(
            ushort clipIndex,
            float speed,
            bool loop,
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<SingleClipStateMachineState> singleClips,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var singleClipState = new SingleClipStateMachineState();

            var newSamplers = new NativeArray<ClipSampler>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            newSamplers[0] = new ClipSampler
            {
                ClipIndex = clipIndex,
                Clips = clips,
                ClipEventsBlob = clipEvents,
                PreviousTime = 0,
                Time = 0,
                Weight = 0
            };

            var playableIndex = PlayableState.New(ref playableStates, ref samplers, newSamplers, speed, loop);
            singleClipState.PlayableId = playableStates[playableIndex].Id;
            singleClips.Add(singleClipState);
            return singleClipState;
        }

        public void UpdateSamplers(float dt,
            in PlayableState playable,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var samplerIndex = samplers.IdToIndex(playable.StartSamplerId);
            var sampler = samplers[samplerIndex];
            sampler.Weight = playable.Weight;

            sampler.PreviousTime = sampler.Time;
            sampler.Time += dt * playable.Speed;
            if (playable.Loop)
            {
                sampler.LoopToClipTime();
            }
            samplers[samplerIndex] = sampler;
        }
    }
}