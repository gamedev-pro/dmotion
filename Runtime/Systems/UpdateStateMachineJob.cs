using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct UpdateStateMachineJob : IJobEntity
    {
        public float DeltaTime;
        public void Execute(
            ref AnimationStateMachine stateMachine,
            ref DynamicBuffer<ClipSampler> samplers,
            in DynamicBuffer<AnimationState> states,
            in DynamicBuffer<AnimationTransitionGroup> transitionsGroups,
            in DynamicBuffer<BoolTransition> boolTransitions,
            in DynamicBuffer<BoolParameter> boolParameters,
            in DynamicBuffer<ExitTimeTransition> exitTimeTransitions
            )
        {
            AnimationStateMachineUtils.RaiseExceptionIfNotValid(stateMachine, states);
            //Reset NextState if requested (TODO: better way to do this?)
            {
                if (stateMachine.RequestedNextState.IsValid)
                {
                    stateMachine.NextState = stateMachine.RequestedNextState;
                    var nextState = states[stateMachine.NextState.StateIndex];
                    nextState.ResetTime(ref samplers);
                    stateMachine.RequestedNextState = AnimationStateMachine.StateRef.Null;
                }
            }
            
            //Evaluate if current transition ended
            {
                if (states.IsValidIndex(stateMachine.NextState.StateIndex))
                {
                    var nextState = states[stateMachine.NextState.StateIndex];
                    if (nextState.GetStateTime(samplers) > nextState.TransitionDuration)
                    {
                        if (!stateMachine.CurrentState.IsOneShot)
                        {
                            stateMachine.PrevState = stateMachine.CurrentState;
                        }

                        stateMachine.CurrentState = stateMachine.NextState;
                        stateMachine.NextState = AnimationStateMachine.StateRef.Null;
                    }
                }
            }
            
            //Evaluate transitions
            {
                var stateToEvaluate = stateMachine.NextState.IsValid ? stateMachine.NextState : stateMachine.CurrentState;
                var prevState = stateMachine.NextState.IsValid ? stateMachine.CurrentState : stateMachine.PrevState;

                var shouldStartTransition = EvaluateTransitions(stateToEvaluate.StateIndex, transitionsGroups,
                    boolTransitions, boolParameters, out var nextStateIndex);

                if (!shouldStartTransition)
                {
                    //evaluate exit time transitions
                    var stateTime = states[stateToEvaluate.StateIndex].GetStateTime(samplers);
                    shouldStartTransition =
                        EvaluateExitTimeTransitions(stateToEvaluate.StateIndex, prevState.StateIndex, stateTime,
                            exitTimeTransitions, out nextStateIndex);
                }

                if (shouldStartTransition)
                {
                    stateMachine.RequestedNextState = new AnimationStateMachine.StateRef() { StateIndex = nextStateIndex };
                }
            }
            
            //Update samplers
            {
                var currentState = states.ElementAtSafe(stateMachine.CurrentState.StateIndex);
                currentState.Update(DeltaTime, ref samplers);
                if (stateMachine.NextState.IsValid)
                {
                    var nextState = states.ElementAtSafe(stateMachine.NextState.StateIndex);
                    nextState.Update(DeltaTime, ref samplers);
                }
            }
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitions(
            int stateIndex,
            in DynamicBuffer<AnimationTransitionGroup> transitionGroups,
            in DynamicBuffer<BoolTransition> boolTransitions,
            in DynamicBuffer<BoolParameter> boolParameters,
            out int nextStateIndex)
        {
            for (var i = 0; i < transitionGroups.Length; i++)
            {
                var group = transitionGroups[i];
                if (group.FromStateIndex == stateIndex)
                {
                    if (EvaluateTransitionGroup(i, group, boolTransitions, boolParameters))
                    {
                        nextStateIndex = group.ToStateIndex;
                        return true;
                    }
                }
            }

            nextStateIndex = -1;
            return false;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitionGroup(int groupIndex, in AnimationTransitionGroup group,
            in DynamicBuffer<BoolTransition> boolTransitions, in DynamicBuffer<BoolParameter> boolParameters)
        {
            var shouldTriggerTransition = boolTransitions.Length > 0;
            for (var i = 0; i < boolTransitions.Length; i++)
            {
                var transition = boolTransitions[i];
                if (transition.GroupIndex == groupIndex)
                {
                    shouldTriggerTransition &= transition.Evaluate(boolParameters[transition.ParameterIndex]);
                }
            }

            return shouldTriggerTransition;
        }

        [BurstCompile]
        private static bool EvaluateExitTimeTransitions(
            int stateIndex,
            int prevStateIndex,
            float stateTime,
            in DynamicBuffer<ExitTimeTransition> exitTimeTransitions,
            out int nextStateIndex)
        {
            for (var i = 0; i < exitTimeTransitions.Length; i++)
            {
                var transition = exitTimeTransitions[i];
                if (transition.FromStateIndex == stateIndex)
                {
                    if (stateTime > transition.NormalizedExitTime)
                    {
                        nextStateIndex = transition.IsTransitionToStateMachine
                            ? prevStateIndex
                            : transition.ToStateIndex;
                        return true;
                    }
                }
            }

            nextStateIndex = -1;
            return false;
        }
    }
}