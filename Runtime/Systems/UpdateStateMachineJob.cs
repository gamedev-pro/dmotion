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
            ref AnimationStateMachineTransitionRequest stateMachineTransitionRequest,
            ref PlayableTransitionRequest playableTransitionRequest,
            ref DynamicBuffer<SingleClipState> singleClipStates,
            ref DynamicBuffer<LinearBlendAnimationStateMachineState> linearBlendStates,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            ref DynamicBuffer<PlayableState> playableStates,
            in PlayableCurrentState playableCurrentState,
            in DynamicBuffer<BoolParameter> boolParameters
        )
        {
            using var scope = Marker.Auto();
            ref var stateMachineBlob = ref stateMachine.StateMachineBlob.Value;

            var shouldStateMachineBeActive = !playableCurrentState.IsValid ||
                                             stateMachineTransitionRequest.IsRequested ||
                                             playableCurrentState.PlayableId == stateMachine.CurrentState.PlayableId;

            if (!shouldStateMachineBeActive)
            {
                return;
            }

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

                    playableTransitionRequest = new PlayableTransitionRequest
                    {
                        PlayableId = stateMachine.CurrentState.PlayableId,
                        TransitionDuration = 0
                    };

                    //We already started a transition to a new state 
                    stateMachineTransitionRequest = AnimationStateMachineTransitionRequest.Null;
                }
            }

            //Evaluate if we should transition into the state machine
            {
                if (stateMachineTransitionRequest.IsRequested)
                {
                    var isCurrentStateActive = playableCurrentState.PlayableId == stateMachine.CurrentState.PlayableId;
                    // we're already playing our current state
                    if (!isCurrentStateActive)
                    {
                        var isCurrentStatePlayableAlive =
                            playableStates.ExistsWithId((byte)stateMachine.CurrentState.PlayableId);
                        
                        if (!isCurrentStatePlayableAlive)
                        {
                            //create a new playable state for us to transition to
                            stateMachine.CurrentState = CreateState(
                                (short)stateMachine.CurrentState.StateIndex,
                                stateMachine.StateMachineBlob,
                                stateMachine.ClipsBlob,
                                stateMachine.ClipEventsBlob,
                                ref singleClipStates,
                                ref linearBlendStates,
                                ref playableStates,
                                ref clipSamplers);
                        }

                        playableTransitionRequest = new PlayableTransitionRequest
                        {
                            PlayableId = stateMachine.CurrentState.PlayableId,
                            TransitionDuration = stateMachineTransitionRequest.TransitionDuration
                        };
                    }
                    
                    stateMachineTransitionRequest = AnimationStateMachineTransitionRequest.Null;
                }
            }

            //Evaluate transitions
            {
                //we really expect this guy to exist
                var currentStatePlayable = 
                    playableStates.GetWithId((byte) stateMachine.CurrentState.PlayableId);
                
                var shouldStartTransition = EvaluateTransitions(
                    currentStatePlayable,
                    ref stateMachine.CurrentStateBlob,
                    boolParameters,
                    out var transitionIndex);

                if (shouldStartTransition)
                {
                    ref var transition = ref stateMachine.CurrentStateBlob.Transitions[transitionIndex];
                    stateMachine.CurrentState = CreateState(
                        transition.ToStateIndex,
                        stateMachine.StateMachineBlob,
                        stateMachine.ClipsBlob,
                        stateMachine.ClipEventsBlob,
                        ref singleClipStates,
                        ref linearBlendStates,
                        ref playableStates,
                        ref clipSamplers);

                    playableTransitionRequest = new PlayableTransitionRequest
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
            ref DynamicBuffer<SingleClipState> singleClipStates,
            ref DynamicBuffer<LinearBlendAnimationStateMachineState> linearBlendStates,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var state = ref stateMachineBlob.Value.States[stateIndex];
            var stateRef = new StateMachineStateRef
            {
                StateIndex = (ushort)stateIndex
            };

            byte playableId;
            switch (state.Type)
            {
                case StateType.Single:
                    var singleClipState = SingleClipState.NewForStateMachine(
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
        private bool EvaluateTransitions(in PlayableState playable, ref AnimationStateBlob state,
            in DynamicBuffer<BoolParameter> boolParameters, out short transitionIndex)
        {
            for (short i = 0; i < state.Transitions.Length; i++)
            {
                if (EvaluateTransitionGroup(playable, ref state.Transitions[i], boolParameters))
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
        private bool EvaluateTransitionGroup(in PlayableState playable, ref StateOutTransitionGroup transitionGroup,
            in DynamicBuffer<BoolParameter> boolParameters)
        {
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