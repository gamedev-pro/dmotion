using Latios.Kinemation;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace DMotion
{
    [BurstCompile]
    internal struct SingleClipStateMachineState : IBufferElementData
    {
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal short StateIndex;
        internal byte PlayableId;
        internal readonly ref SingleClipStateBlob AsSingleClip =>
            ref StateMachineBlob.Value.SingleClipStates[StateBlob.StateIndex];

        internal readonly ref AnimationStateBlob StateBlob => ref StateMachineBlob.Value.States[StateIndex];
        internal readonly StateType Type => StateBlob.Type;

        public static SingleClipStateMachineState New(
            short stateIndex,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clips,
            BlobAssetReference<ClipEventsBlob> clipEvents,
            ref DynamicBuffer<SingleClipStateMachineState> singleClips,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var singleClipState = new SingleClipStateMachineState
            {
                StateMachineBlob = stateMachineBlob,
                StateIndex = stateIndex
            };

            var newSamplers = new NativeArray<ClipSampler>(1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            newSamplers[0] = new ClipSampler
            {
                ClipIndex = singleClipState.AsSingleClip.ClipIndex,
                Clips = clips,
                ClipEventsBlob = clipEvents,
                PreviousTime = 0,
                Time = 0,
                Weight = 0
            };
            
            var playableIndex = PlayableState.New(ref playableStates, ref samplers, newSamplers, singleClipState.StateBlob.Speed,
                singleClipState.StateBlob.Loop);
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