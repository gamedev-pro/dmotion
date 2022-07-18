using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct UpdateStateMachineJob : IJobEntity
    {
        internal float DeltaTime;
        internal void Execute(
            ref AnimationStateMachine stateMachine,
            in DynamicBuffer<BoolParameter> boolParameters
            )
        {
            //Evaluate if current transition ended
            {
                if (stateMachine.CurrentTransition.IsValid)
                {
                    if (stateMachine.NextState.NormalizedTime > stateMachine.CurrentTransitionBlob.NormalizedTransitionDuration)
                    {
                        stateMachine.CurrentState = stateMachine.NextState;
                        stateMachine.NextState = AnimationState.Null;
                        stateMachine.CurrentTransition = StateTransition.Null;
                    }
                }
            }
            
            //Evaluate transitions
            {
                var stateToEvaluate = stateMachine.CurrentTransition.IsValid
                    ? stateMachine.NextState.StateIndex
                    : stateMachine.CurrentState.StateIndex;
                
                var shouldStartTransition = EvaluateTransitions(stateMachine, stateToEvaluate, boolParameters,
                    out var transitionIndex);

                if (shouldStartTransition)
                {
                    stateMachine.CurrentTransition = new StateTransition() { TransitionIndex = transitionIndex };
                    stateMachine.NextState = stateMachine.CreateState(stateMachine.CurrentTransitionBlob.ToStateIndex);
                }
            }
            
            //Update samplers
            {
                stateMachine.CurrentState.Update(DeltaTime);
                if (stateMachine.CurrentTransition.IsValid)
                {
                    stateMachine.NextState.Update(DeltaTime);
                }
            }
            
            //Sync parameters
            // for (var i = 0; i < blendParameters.Length; i++)
            // {
            //     var blend = blendParameters[i];
            //     var stateIndex = blend.StateIndex == stateMachine.CurrentState.StateIndex
            //         ? stateMachine.CurrentState.StateIndex
            //         : blend.StateIndex == stateMachine.NextState.StateIndex
            //             ? stateMachine.NextState.StateIndex
            //             : - 1;
            //     if (stateIndex >= 0)
            //     {
            //         var state = states[stateIndex];
            //         state.Blend = blend.Value;
            //         states[stateIndex] = state;
            //     }
            // }
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitions(in AnimationStateMachine stateMachine, short stateToEvaluate,
            in DynamicBuffer<BoolParameter> boolParameters, out short transitionIndex)
        {
            for (short i = 0; i < stateMachine.TransitionsBlob.Length; i++)
            {
                ref var group = ref stateMachine.TransitionsBlob[i];
                if (group.FromStateIndex == stateToEvaluate)
                {
                    if (EvaluateTransitionGroup(i, ref group, boolParameters))
                    {
                        transitionIndex = i;
                        return true;
                    }
                }
            }
            transitionIndex = -1;
            return false;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitionGroup(short groupIndex, ref AnimationTransitionGroup group,
            in DynamicBuffer<BoolParameter> boolParameters)
        {
            ref var boolTransitions = ref group.BoolTransitions;
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
    }
}