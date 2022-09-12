using System;
using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;

namespace DMotion
{
    [BurstCompile]
    internal partial struct UpdateSingleClipStateMachineStatesJob : IJobEntity
    {
        internal float DeltaTime;
        internal ProfilerMarker Marker;

        internal void Execute(
            ref DynamicBuffer<SingleClipStateMachineState> singleClipStates,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> clipSamplers
        )
        {
            for (var i = 0; i < singleClipStates.Length; i++)
            {
                if (playableStates.TryGetWithId(singleClipStates[i].PlayableId, out var playable))
                {
                    singleClipStates[i]
                        .UpdateSamplers(DeltaTime, playable.Weight, ref playableStates, ref clipSamplers);
                }
            }
        }
    }

    [BurstCompile]
    internal partial struct UpdateLinearBlendStateMachineStatesJob : IJobEntity
    {
        internal float DeltaTime;
        internal ProfilerMarker Marker;

        internal void Execute(
            ref DynamicBuffer<LinearBlendAnimationStateMachineState> linearBlendStates,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            in DynamicBuffer<BlendParameter> blendParameters
        )
        {
            for (var i = 0; i < linearBlendStates.Length; i++)
            {
                if (playableStates.TryGetWithId(linearBlendStates[i].PlayableId, out var playable))
                {
                    linearBlendStates[i].UpdateSamplers(DeltaTime, playable.Weight, blendParameters, ref playableStates,
                        ref clipSamplers);
                }
            }
        }
    }

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
                }
            }

            //Evaluate if current transition ended
            {
                if (stateMachine.NextState.IsValid)
                {
                    var nextStatePlayableIndex = stateMachine.NextState.IdToIndex(playableStates);
                    if (playableStates[nextStatePlayableIndex].Time > stateMachine.CurrentTransitionDuration)
                    {
                        DestroyState(stateMachine.CurrentState,
                            ref singleClipStates, ref linearBlendStates,
                            ref playableStates, ref clipSamplers);
                        stateMachine.CurrentState = stateMachine.NextState;
                        stateMachine.NextState = StateMachineStateRef.Null;
                    }
                }
            }

            //Evaluate transitions
            {
                var stateToEvaluate = stateMachine.NextState.IsValid
                    ? stateMachine.NextState
                    : stateMachine.CurrentState;

                var shouldStartTransition = EvaluateTransitions(stateToEvaluate, playableStates, boolParameters,
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
                        ref singleClipStates,
                        ref linearBlendStates,
                        ref playableStates,
                        ref clipSamplers);
                }
            }

            //Update samplers
            {
                if (stateMachine.NextState.IsValid)
                {
                    var currentStatePlayableIndex = stateMachine.CurrentState.IdToIndex(playableStates);
                    var nextStatePlayableIndex = stateMachine.NextState.IdToIndex(playableStates);

                    var currentPlayableState = playableStates[currentStatePlayableIndex];
                    var nextPlayableState = playableStates[nextStatePlayableIndex];

                    var nextStateBlend = math.clamp(nextPlayableState.Time /
                                                    stateMachine.CurrentTransitionDuration, 0, 1);

                    currentPlayableState.Weight = (1 - nextStateBlend) * stateMachine.Weight;
                    nextPlayableState.Weight = nextStateBlend * stateMachine.Weight;

                    playableStates[currentStatePlayableIndex] = currentPlayableState;
                    playableStates[nextStatePlayableIndex] = nextPlayableState;
                }
                else
                {
                    var currentStatePlayableIndex = stateMachine.CurrentState.IdToIndex(playableStates);
                    var currentPlayableState = playableStates[currentStatePlayableIndex];
                    currentPlayableState.Weight = stateMachine.Weight;
                    playableStates[currentStatePlayableIndex] = currentPlayableState;
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


        private void DestroyState(StateMachineStateRef stateRef,
            ref DynamicBuffer<SingleClipStateMachineState> singleClipStates,
            ref DynamicBuffer<LinearBlendAnimationStateMachineState> linearBlendStates,
            ref DynamicBuffer<PlayableState> playableStates,
            ref DynamicBuffer<ClipSampler> clipSamplers)
        {
            PlayableState.DestroyStateWithId(ref playableStates, ref clipSamplers, (byte) stateRef.PlayableId);
            switch (stateRef.Type)
            {
                case StateType.Single:
                    for (int i = 0; i < singleClipStates.Length; i++)
                    {
                        if (singleClipStates[i].PlayableId == stateRef.PlayableId)
                        {
                            singleClipStates.RemoveAtSwapBack(i);
                            break;
                        }
                    }

                    break;
                case StateType.LinearBlend:
                    for (int i = 0; i < linearBlendStates.Length; i++)
                    {
                        if (linearBlendStates[i].PlayableId == stateRef.PlayableId)
                        {
                            linearBlendStates.RemoveAtSwapBack(i);
                            break;
                        }
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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