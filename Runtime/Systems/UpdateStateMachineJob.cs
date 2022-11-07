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
            ref AnimationStateTransitionRequest animationStateTransitionRequest,
            ref DynamicBuffer<SingleClipState> singleClipStates,
            ref DynamicBuffer<LinearBlendStateMachineState> linearBlendStates,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            ref DynamicBuffer<AnimationState> animationStates,
            in AnimationCurrentState animationCurrentState,
            in AnimationStateTransition animationStateTransition,
            in DynamicBuffer<BoolParameter> boolParameters,
            in DynamicBuffer<IntParameter> intParameters
        )
        {
            using var scope = Marker.Auto();
            ref var stateMachineBlob = ref stateMachine.StateMachineBlob.Value;

            if (!ShouldStateMachineBeActive(animationCurrentState, animationStateTransition, stateMachine.CurrentState))
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
                }
            }

            //Evaluate transitions
            {
                //we really expect this guy to exist
                var currentStateAnimationState =
                    animationStates.GetWithId((byte)stateMachine.CurrentState.AnimationStateId);

                var shouldStartTransition = EvaluateTransitions(
                    currentStateAnimationState,
                    ref stateMachine.CurrentStateBlob,
                    boolParameters,
                    intParameters,
                    out var transitionIndex);

                if (shouldStartTransition)
                {
                    ref var transition = ref stateMachine.CurrentStateBlob.Transitions[transitionIndex];

#if UNITY_EDITOR || DEBUG
                    stateMachine.PreviousState = stateMachine.CurrentState;
#endif
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

        public static bool ShouldStateMachineBeActive(in AnimationCurrentState animationCurrentState,
            in AnimationStateTransition animationStateTransition,
            in StateMachineStateRef currentState)
        {
            return !animationCurrentState.IsValid ||
                   (
                       currentState.IsValid && animationCurrentState.IsValid &&
                       animationCurrentState.AnimationStateId ==
                       currentState.AnimationStateId
                   ) ||
                   (
                       currentState.IsValid && animationStateTransition.IsValid &&
                       animationStateTransition.AnimationStateId ==
                       currentState.AnimationStateId
                   );
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
                        (byte)stateIndex,
                        stateMachineBlob,
                        clipsBlob,
                        clipEventsBlob,
                        ref singleClipStates,
                        ref animationStates, ref samplers);
                    animationStateId = singleClipState.AnimationStateId;
                    break;
                case StateType.LinearBlend:
                    var linearClipState = LinearBlendStateUtils.NewForStateMachine(
                        (byte)stateIndex,
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
            in DynamicBuffer<BoolParameter> boolParameters,
            in DynamicBuffer<IntParameter> intParameters,
            out short transitionIndex)
        {
            for (short i = 0; i < state.Transitions.Length; i++)
            {
                if (EvaluateTransitionGroup(animation, ref state.Transitions[i], boolParameters, intParameters))
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
            in DynamicBuffer<BoolParameter> boolParameters,
            in DynamicBuffer<IntParameter> intParameters)
        {
            if (transitionGroup.HasEndTime && animation.Time < transitionGroup.TransitionEndTime)
            {
                return false;
            }

            var shouldTriggerTransition = transitionGroup.HasAnyConditions || transitionGroup.HasEndTime;

            //evaluate bool transitions
            {
                ref var boolTransitions = ref transitionGroup.BoolTransitions;
                for (var i = 0; i < boolTransitions.Length; i++)
                {
                    var transition = boolTransitions[i];
                    shouldTriggerTransition &= transition.Evaluate(boolParameters[transition.ParameterIndex]);
                }
            }
            //evaluate int transitions
            {
                ref var intTransitions = ref transitionGroup.IntTransitions;
                for (var i = 0; i < intTransitions.Length; i++)
                {
                    var transition = intTransitions[i];
                    shouldTriggerTransition &= transition.Evaluate(intParameters[transition.ParameterIndex]);
                }
            }

            return shouldTriggerTransition;
        }
    }
}