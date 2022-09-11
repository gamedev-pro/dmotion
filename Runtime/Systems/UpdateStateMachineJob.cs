using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;

namespace DMotion
{
    [BurstCompile]
    internal partial struct UpdateStateMachineJob : IJobEntity
    {
        internal float DeltaTime;
        internal ProfilerMarker Marker;

        internal void Execute(
            ref AnimationStateMachine stateMachine,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            in DynamicBuffer<BlendParameter> blendParameters,
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
                        ref clipSamplers);
                }
            }
            //Evaluate if current transition ended
            {
                if (stateMachine.NextState.IsValid)
                {
                    if (stateMachine.NextState.Time > stateMachine.CurrentTransitionDuration)
                    {
                        var removeCount = stateMachine.CurrentState.ClipCount;
                        clipSamplers.RemoveRangeWithId(stateMachine.CurrentState.StartSamplerId, removeCount);
                        stateMachine.CurrentState = stateMachine.NextState;
                        stateMachine.NextState = AnimationState.Null;
                    }
                }
            }

            //Evaluate transitions
            {
                var stateToEvaluate = stateMachine.NextState.IsValid
                    ? stateMachine.NextState
                    : stateMachine.CurrentState;

                var shouldStartTransition = EvaluateTransitions(stateToEvaluate, boolParameters,
                    out var transitionIndex);

                if (shouldStartTransition)
                {
                    ref var transition = ref stateToEvaluate.StateBlob.Transitions[transitionIndex];
                    stateMachine.CurrentTransitionDuration = transition.TransitionDuration;
                    stateMachine.NextState = CreateState(
                        transition.ToStateIndex,
                        stateMachine.StateMachineBlob,
                        stateMachine.ClipsBlob,
                        stateMachine.ClipEventsBlob,
                        ref clipSamplers);
                }
            }

            //Update samplers
            {
                if (stateMachine.NextState.IsValid)
                {
                    var nextStateBlend = math.clamp(stateMachine.NextState.Time /
                                                    stateMachine.CurrentTransitionDuration, 0, 1);
                    stateMachine.CurrentState.UpdateSamplers(
                        DeltaTime, (1 - nextStateBlend) * stateMachine.Weight,
                        blendParameters, ref clipSamplers);
                    stateMachine.NextState.UpdateSamplers(
                        DeltaTime, nextStateBlend * stateMachine.Weight,
                        blendParameters, ref clipSamplers);
                }
                else
                {
                    stateMachine.CurrentState.UpdateSamplers(DeltaTime, stateMachine.Weight, blendParameters,
                        ref clipSamplers);
                }
            }
        }

        private AnimationState CreateState(short stateIndex,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clipsBlob,
            BlobAssetReference<ClipEventsBlob> clipEventsBlob,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var state = new AnimationState
            {
                StateMachineBlob = stateMachineBlob,
                StateIndex = stateIndex,
                Time = 0,
            };
            state.Initialize(clipsBlob, clipEventsBlob, ref samplers);
            return state;
        }

        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitions(in AnimationState state,
            in DynamicBuffer<BoolParameter> boolParameters, out short transitionIndex)
        {
            for (short i = 0; i < state.StateBlob.Transitions.Length; i++)
            {
                if (EvaluateTransitionGroup(state, ref state.StateBlob.Transitions[i], boolParameters))
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
        private bool EvaluateTransitionGroup(in AnimationState state, ref StateOutTransitionGroup transitionGroup,
            in DynamicBuffer<BoolParameter> boolParameters)
        {
            if (transitionGroup.HasEndTime && state.Time < transitionGroup.TransitionEndTime)
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