using System;
using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;

namespace DMotion
{
    [BurstCompile]
    internal partial struct UpdateStateMachineJob : IJobEntity
    {
        internal ProfilerMarker Marker;

        internal void Execute(
            ref AnimationStateMachine stateMachine,
            ref DynamicBuffer<SingleClipStateMachineState> singleClipStates,
            ref DynamicBuffer<LinearBlendAnimationStateMachineState> linearBlendStates,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            ref DynamicBuffer<PlayableState> playableStates,
            ref PlayableTransitionRequest transitionRequest,
            in DynamicBuffer<BoolParameter> boolParameters
        )
        {
            using var scope = Marker.Auto();
            ref var stateMachineBlob = ref stateMachine.StateMachineBlob.Value;

            //Initialize if necessary
            {
                if (!stateMachine.CurrentState.IsValid)
                {
                    stateMachine.CurrentState = CreateState(
                        stateMachineBlob.DefaultStateIndex,
                        stateMachine.StateMachineBlob,
                        stateMachine.ClipsBlob,
                        stateMachine.ClipEventsBlob,
                        ref singleClipStates,
                        ref linearBlendStates,
                        ref playableStates,
                        ref clipSamplers);

                    transitionRequest = new PlayableTransitionRequest()
                    {
                        PlayableId = stateMachine.CurrentState.PlayableId,
                        TransitionDuration = 0
                    };
                }
            }

            //Evaluate transitions
            {
                var shouldStartTransition = EvaluateTransitions(stateMachine.CurrentState, playableStates, boolParameters,
                    out var transitionIndex);

                if (shouldStartTransition)
                {
                    ref var transition = ref stateMachine.CurrentState.StateBlob.Transitions[transitionIndex];
                    stateMachine.CurrentState = CreateState(
                        transition.ToStateIndex,
                        stateMachine.StateMachineBlob,
                        stateMachine.ClipsBlob,
                        stateMachine.ClipEventsBlob,
                        ref singleClipStates,
                        ref linearBlendStates,
                        ref playableStates,
                        ref clipSamplers);

                    transitionRequest = new PlayableTransitionRequest
                    {
                        PlayableId = stateMachine.CurrentState.PlayableId,
                        TransitionDuration = transition.TransitionDuration,
                    };
                }
            }
        }

        private StateMachineStateRef CreateState(short stateIndex,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clipsBlob,
            BlobAssetReference<ClipEventsBlob> clipEventsBlob,
            ref DynamicBuffer<SingleClipStateMachineState> singleClipStates,
            ref DynamicBuffer<LinearBlendAnimationStateMachineState> linearBlendStates,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var state = ref stateMachineBlob.Value.States[stateIndex];
            var stateRef = new StateMachineStateRef
            {
                StateMachineBlob = stateMachineBlob,
                StateIndex = (ushort)stateIndex
            };

            byte playableId;
            switch (state.Type)
            {
                case StateType.Single:
                    var singleClipState = SingleClipStateMachineState.New(
                        (byte)state.StateIndex,
                        stateMachineBlob,
                        clipsBlob,
                        clipEventsBlob,
                        ref singleClipStates,
                        ref playableStates, ref samplers);
                    playableId = singleClipState.PlayableId;
                    break;
                case StateType.LinearBlend:
                    var linearClipState = LinearBlendAnimationStateMachineState.New(
                        (byte)state.StateIndex,
                        stateMachineBlob,
                        clipsBlob,
                        clipEventsBlob,
                        ref linearBlendStates,
                        ref playableStates, ref samplers);

                    playableId = linearClipState.PlayableId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            stateRef.PlayableId = (sbyte)playableId;
            return stateRef;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitions(in StateMachineStateRef state,
            in DynamicBuffer<PlayableState> playableStates,
            in DynamicBuffer<BoolParameter> boolParameters, out short transitionIndex)
        {
            for (short i = 0; i < state.StateBlob.Transitions.Length; i++)
            {
                if (EvaluateTransitionGroup(state, ref state.StateBlob.Transitions[i], playableStates, boolParameters))
                {
                    transitionIndex = i;
                    return true;
                }
            }

            transitionIndex = -1;
            return false;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitionGroup(in StateMachineStateRef state, ref StateOutTransitionGroup transitionGroup,
            in DynamicBuffer<PlayableState> playableStates,
            in DynamicBuffer<BoolParameter> boolParameters)
        {
            var playable = playableStates[state.IdToIndex(playableStates)];
            if (transitionGroup.HasEndTime && playable.Time < transitionGroup.TransitionEndTime)
            {
                return false;
            }

            ref var boolTransitions = ref transitionGroup.BoolTransitions;
            var shouldTriggerTransition = transitionGroup.HasAnyConditions || transitionGroup.HasEndTime;
            for (var i = 0; i < boolTransitions.Length; i++)
            {
                var transition = boolTransitions[i];
                shouldTriggerTransition &= transition.Evaluate(boolParameters[transition.ParameterIndex]);
            }

            return shouldTriggerTransition;
        }
    }
}