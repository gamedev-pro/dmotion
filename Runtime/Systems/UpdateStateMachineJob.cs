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
            ref AnimationStateTransitionRequest animationStateTransitionRequest,
            ref DynamicBuffer<SingleClipState> singleClipStates,
            ref DynamicBuffer<LinearBlendStateMachineState> linearBlendStates,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            ref DynamicBuffer<AnimationState> animationStates,
            in AnimationCurrentState animationCurrentState,
            in DynamicBuffer<BoolParameter> boolParameters
        )
        {
            using var scope = Marker.Auto();
            ref var stateMachineBlob = ref stateMachine.StateMachineBlob.Value;

            var shouldStateMachineBeActive = !animationCurrentState.IsValid ||
                                             stateMachineTransitionRequest.IsRequested ||
                                             animationCurrentState.AnimationStateId == stateMachine.CurrentState.AnimationStateId;

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
                        ref animationStates,
                        ref clipSamplers);

                    animationStateTransitionRequest = new AnimationStateTransitionRequest
                    {
                        AnimationStateId = stateMachine.CurrentState.AnimationStateId,
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
                    var isCurrentStateActive = animationCurrentState.AnimationStateId == stateMachine.CurrentState.AnimationStateId;
                    // we're already playing our current state
                    if (!isCurrentStateActive)
                    {
                        var isCurrentStateAnimationStateAlive =
                            animationStates.ExistsWithId((byte)stateMachine.CurrentState.AnimationStateId);
                        
                        if (!isCurrentStateAnimationStateAlive)
                        {
                            //create a new animationState state for us to transition to
                            stateMachine.CurrentState = CreateState(
                                (short)stateMachine.CurrentState.StateIndex,
                                stateMachine.StateMachineBlob,
                                stateMachine.ClipsBlob,
                                stateMachine.ClipEventsBlob,
                                ref singleClipStates,
                                ref linearBlendStates,
                                ref animationStates,
                                ref clipSamplers);
                        }

                        animationStateTransitionRequest = new AnimationStateTransitionRequest
                        {
                            AnimationStateId = stateMachine.CurrentState.AnimationStateId,
                            TransitionDuration = stateMachineTransitionRequest.TransitionDuration
                        };
                    }
                    
                    stateMachineTransitionRequest = AnimationStateMachineTransitionRequest.Null;
                }
            }

            //Evaluate transitions
            {
                //we really expect this guy to exist
                var currentStateAnimationState = 
                    animationStates.GetWithId((byte) stateMachine.CurrentState.AnimationStateId);
                
                var shouldStartTransition = EvaluateTransitions(
                    currentStateAnimationState,
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
                        ref animationStates,
                        ref clipSamplers);

                    animationStateTransitionRequest = new AnimationStateTransitionRequest
                    {
                        AnimationStateId = stateMachine.CurrentState.AnimationStateId,
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
            ref DynamicBuffer<LinearBlendStateMachineState> linearBlendStates,
            ref DynamicBuffer<AnimationState> animationStates,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            ref var state = ref stateMachineBlob.Value.States[stateIndex];
            var stateRef = new StateMachineStateRef
            {
                StateIndex = (ushort)stateIndex
            };

            byte animationStateId;
            switch (state.Type)
            {
                case StateType.Single:
                    var singleClipState = SingleClipStateUtils.NewForStateMachine(
                        (byte)state.StateIndex,
                        stateMachineBlob,
                        clipsBlob,
                        clipEventsBlob,
                        ref singleClipStates,
                        ref animationStates, ref samplers);
                    animationStateId = singleClipState.AnimationStateId;
                    break;
                case StateType.LinearBlend:
                    var linearClipState = LinearBlendStateUtils.NewForStateMachine(
                        (byte)state.StateIndex,
                        stateMachineBlob,
                        clipsBlob,
                        clipEventsBlob,
                        ref linearBlendStates,
                        ref animationStates, ref samplers);

                    animationStateId = linearClipState.AnimationStateId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            stateRef.AnimationStateId = (sbyte)animationStateId;
            return stateRef;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitions(in AnimationState animation, ref AnimationStateBlob state,
            in DynamicBuffer<BoolParameter> boolParameters, out short transitionIndex)
        {
            for (short i = 0; i < state.Transitions.Length; i++)
            {
                if (EvaluateTransitionGroup(animation, ref state.Transitions[i], boolParameters))
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
        private bool EvaluateTransitionGroup(in AnimationState animation, ref StateOutTransitionGroup transitionGroup,
            in DynamicBuffer<BoolParameter> boolParameters)
        {
            if (transitionGroup.HasEndTime && animation.Time < transitionGroup.TransitionEndTime)
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