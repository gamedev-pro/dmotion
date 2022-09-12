using Latios.Kinemation;
using Unity.Burst;
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

            var playableIndex = PlayableState.New(ref playableStates, singleClipState.StateBlob.Speed,
                singleClipState.StateBlob.Loop);

            var playableState = playableStates[playableIndex];
            singleClipState.PlayableId = playableState.Id;

            playableState.StartSamplerId = samplers.AddWithId(new ClipSampler
            {
                ClipIndex = singleClipState.AsSingleClip.ClipIndex,
                Clips = clips,
                ClipEventsBlob = clipEvents,
                PreviousTime = 0,
                Time = 0,
                Weight = 0
            });

            playableStates[playableIndex] = playableState;

            singleClips.Add(singleClipState);
            return singleClipState;
        }

        public void UpdateSamplers(float dt, float blendWeight,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var playableIndex = playableStates.IdToIndex(PlayableId);
            var playable = playableStates[playableIndex];
            playable.UpdateTime(dt);

            var samplerIndex = samplers.IdToIndex(playable.StartSamplerId);
            var sampler = samplers[samplerIndex];
            sampler.Weight = blendWeight;

            sampler.PreviousTime = sampler.Time;
            sampler.Time += dt * playableStates[playableIndex].Speed;
            if (playableStates[playableIndex].Loop)
            {
                sampler.LoopToClipTime();
            }

            samplers[samplerIndex] = sampler;
            playableStates[playableIndex] = playable;
        }
    }
}