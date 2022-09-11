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
            ref PlayOneShotRequest playOneShot,
            ref OneShotState oneShotState,
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

            //Evaluate requested one shot
            {
                if (playOneShot.IsValid)
                {
                    //initialize
                    var clipSampler = new ClipSampler
                    {
                        ClipIndex = (byte)playOneShot.ClipIndex,
                        Clips = playOneShot.Clips,
                        ClipEventsBlob = playOneShot.ClipEvents,
                        Time = 0,
                        PreviousTime = 0,
                        Weight = 1
                    };
                    var newSamplerId = clipSamplers.AddWithId(clipSampler);
                    
                    oneShotState = new OneShotState(newSamplerId,
                        playOneShot.TransitionDuration,
                        playOneShot.EndTime * clipSampler.Clip.duration,
                        playOneShot.Speed);

                    playOneShot = PlayOneShotRequest.Null;
                }
            }
            //Evaluate transitions
            {
                if (!oneShotState.IsValid)
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
            }
            
            //Update One shot
            {
                if (oneShotState.IsValid)
                {
                    var samplerIndex = clipSamplers.IdToIndex((byte)oneShotState.SamplerId);
                    var sampler = clipSamplers[samplerIndex];
                    sampler.PreviousTime = sampler.Time;
                    sampler.Time += DeltaTime * oneShotState.Speed;

                    float oneShotWeight;
                    //blend out
                    if (sampler.Time > oneShotState.EndTime)
                    {
                        var blendOutTime = sampler.Clip.duration - oneShotState.EndTime;
                        if (!mathex.iszero(blendOutTime))
                        {
                            oneShotWeight = math.clamp((sampler.Clip.duration - sampler.Time) /
                                                   blendOutTime, 0, 1);
                        }
                        else
                        {
                            oneShotWeight = 0;
                        }
                    }
                    //blend in
                    else
                    {
                        oneShotWeight = math.clamp(sampler.Time /
                                               oneShotState.TransitionDuration, 0, 1);
                    }

                    sampler.Weight = oneShotWeight;
                    stateMachine.Weight = 1 - oneShotWeight;
                    
                    clipSamplers[samplerIndex] = sampler;
                    
                    //if blend out finished
                    if (sampler.Time >= sampler.Clip.duration)
                    {
                        stateMachine.Weight = 1;
                        clipSamplers.RemoveAt(samplerIndex);
                        oneShotState = OneShotState.Null;
                    }
                }
            }
            //Update samplers
            {
                if (stateMachine.NextState.IsValid)
                {
                    var nextStateBlend = math.clamp(stateMachine.NextState.Time /
                                       stateMachine.CurrentTransitionDuration, 0, 1);
                    stateMachine.CurrentState.UpdateSamplers(
                        DeltaTime, (1 - nextStateBlend)*stateMachine.Weight,
                        blendParameters, ref clipSamplers);
                    stateMachine.NextState.UpdateSamplers(
                        DeltaTime, nextStateBlend*stateMachine.Weight,
                        blendParameters, ref clipSamplers);
                }
                else
                {
                    stateMachine.CurrentState.UpdateSamplers(DeltaTime, stateMachine.Weight, blendParameters, ref clipSamplers);
                }
            }

            //Normalized weights if needed
            {
                var sumWeights = 0.0f;
                for (var i = 0; i < clipSamplers.Length; i++)
                {
                    sumWeights += clipSamplers[i].Weight;
                }
            
                var inverseSumWeights = 1.0f / sumWeights;
                for (var i = 0; i < clipSamplers.Length; i++)
                {
                    var sampler = clipSamplers[i];
                    sampler.Weight *= inverseSumWeights;
                    clipSamplers[i] = sampler;
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