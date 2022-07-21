using System.Runtime.CompilerServices;
using Latios.Kinemation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct UpdateStateMachineJob : IJobEntity
    {
        internal float DeltaTime;
        internal void Execute(
            ref AnimationStateMachine stateMachine,
            ref DynamicBuffer<ClipSampler> clipSamplers,
            ref PlayOneShotRequest playOneShot,
            ref OneShotState oneShotState,
            in DynamicBuffer<BlendParameter> blendParameters,
            in DynamicBuffer<BoolParameter> boolParameters
            )
        {
            ref var stateMachineBlob = ref stateMachine.StateMachineBlob.Value;

            //Initialize if necessary
            {
                if (!stateMachine.CurrentState.IsValid)
                {
                    clipSamplers.Clear();
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
                if (stateMachine.CurrentTransition.IsValid)
                {
                    if (stateMachine.NextState.NormalizedTime > stateMachine.CurrentTransitionBlob.NormalizedTransitionDuration)
                    {
                        var removeCount = stateMachine.CurrentState.ClipCount;
                        clipSamplers.RemoveRange(stateMachine.CurrentState.StartSamplerIndex, removeCount);
                        stateMachine.NextState.StartSamplerIndex -= removeCount;
                        oneShotState.SamplerIndex -= removeCount;
                        
                        stateMachine.CurrentState = stateMachine.NextState;
                        stateMachine.NextState = AnimationState.Null;
                        stateMachine.CurrentTransition = StateTransition.Null;
                    }
                }
            }

            //Evaluate requested one shot
            {
                if (playOneShot.IsValid)
                {
                    //initialize
                    oneShotState = new OneShotState
                    {
                        NormalizedTransitionDuration = playOneShot.NormalizedTransitionDuration,
                        Speed = playOneShot.Speed,
                        SamplerIndex = (short) clipSamplers.Length
                    };

                    clipSamplers.Add(new ClipSampler()
                    {
                        ClipIndex = (byte)playOneShot.ClipIndex,
                        Clips = playOneShot.Clips,
                        ClipEventsBlob = playOneShot.ClipEvents,
                        NormalizedTime = 0,
                        PreviousNormalizedTime = 0,
                        Weight = 1
                    });

                    playOneShot = PlayOneShotRequest.Null;
                }
            }
            //Evaluate transitions
            {
                if (!oneShotState.IsValid)
                {
                    var stateToEvaluate = stateMachine.CurrentTransition.IsValid
                        ? stateMachine.NextState.StateIndex
                        : stateMachine.CurrentState.StateIndex;
                    
                    var shouldStartTransition = EvaluateTransitions(ref stateMachineBlob, stateToEvaluate, boolParameters,
                        out var transitionIndex);

                    if (shouldStartTransition)
                    {
                        stateMachine.CurrentTransition = new StateTransition() { TransitionIndex = transitionIndex };
                        stateMachine.NextState = CreateState(
                            stateMachine.CurrentTransitionBlob.ToStateIndex,
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
                    var sampler = clipSamplers[oneShotState.SamplerIndex];
                    sampler.PreviousNormalizedTime = sampler.NormalizedTime;
                    sampler.NormalizedTime += DeltaTime * oneShotState.Speed;

                    float oneShotWeight;
                    //blend out
                    if (sampler.NormalizedTime > 0.9f)
                    {
                        oneShotWeight = math.clamp((1 - sampler.NormalizedTime) /
                                               oneShotState.NormalizedTransitionDuration, 0, 1);
                    }
                    //blend in
                    else
                    {
                        oneShotWeight = math.clamp(sampler.NormalizedTime /
                                               oneShotState.NormalizedTransitionDuration, 0, 1);
                    }

                    sampler.Weight = oneShotWeight;
                    stateMachine.Weight = 1 - oneShotWeight;
                    
                    clipSamplers[oneShotState.SamplerIndex] = sampler;
                    
                    if (sampler.NormalizedTime >= 1)
                    {
                        stateMachine.Weight = 1;
                        clipSamplers.RemoveAt(oneShotState.SamplerIndex);
                        if (oneShotState.SamplerIndex < stateMachine.CurrentState.StartSamplerIndex)
                        {
                            stateMachine.CurrentState.StartSamplerIndex -= 1;
                        }
                        if (stateMachine.NextState.IsValid && oneShotState.SamplerIndex < stateMachine.NextState.StartSamplerIndex)
                        {
                            stateMachine.NextState.StartSamplerIndex -= 1;
                        }
                        
                        oneShotState = OneShotState.Null;
                    }
                }
            }
            //Update samplers
            {
                if (stateMachine.CurrentTransition.IsValid)
                {
                    var nextStateBlend = math.clamp(stateMachine.NextState.NormalizedTime /
                                       stateMachine.CurrentTransitionBlob.NormalizedTransitionDuration, 0, 1);
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
            var state = new AnimationState()
            {
                StateMachineBlob = stateMachineBlob,
                StateIndex = stateIndex,
                NormalizedTime = 0,
            };
            state.Initialize(clipsBlob, clipEventsBlob, ref samplers);
            return state;
        }
        
        [BurstCompile]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool EvaluateTransitions(ref StateMachineBlob stateMachine, short stateToEvaluate,
            in DynamicBuffer<BoolParameter> boolParameters, out short transitionIndex)
        {
            for (short i = 0; i < stateMachine.Transitions.Length; i++)
            {
                if (stateMachine.Transitions[i].FromStateIndex == stateToEvaluate)
                {
                    if (EvaluateTransitionGroup(i, ref stateMachine, boolParameters))
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
        private bool EvaluateTransitionGroup(short groupIndex, ref StateMachineBlob stateMachine,
            in DynamicBuffer<BoolParameter> boolParameters)
        {
            ref var boolTransitions = ref stateMachine.BoolTransitions;
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