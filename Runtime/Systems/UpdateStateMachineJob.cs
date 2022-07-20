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
                    stateMachine.CurrentState = CreateState(stateMachineBlob.DefaultStateIndex,
                        stateMachine.StateMachineBlob, stateMachine.ClipsBlob,
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
                
                var shouldStartTransition = EvaluateTransitions(ref stateMachineBlob, stateToEvaluate, boolParameters,
                    out var transitionIndex);

                if (shouldStartTransition)
                {
                    stateMachine.CurrentTransition = new StateTransition() { TransitionIndex = transitionIndex };
                    stateMachine.NextState = CreateState(stateMachine.CurrentTransitionBlob.ToStateIndex,
                        stateMachine.StateMachineBlob, stateMachine.ClipsBlob,
                        ref clipSamplers);
                }
            }
            
            //Update samplers
            {
                if (stateMachine.CurrentTransition.IsValid)
                {
                    var nextStateBlend = math.clamp(stateMachine.NextState.NormalizedTime /
                                       stateMachine.CurrentTransitionBlob.NormalizedTransitionDuration, 0, 1);
                    stateMachine.CurrentState.UpdateSamplers(DeltaTime, 1 - nextStateBlend, blendParameters, ref clipSamplers);
                    stateMachine.NextState.UpdateSamplers(DeltaTime, nextStateBlend, blendParameters, ref clipSamplers);
                }
                else
                {
                    stateMachine.CurrentState.UpdateSamplers(DeltaTime, 1, blendParameters, ref clipSamplers);
                }
            }
        }

        private AnimationState CreateState(short stateIndex,
            BlobAssetReference<StateMachineBlob> stateMachineBlob,
            BlobAssetReference<SkeletonClipSetBlob> clipsBlob,
            ref DynamicBuffer<ClipSampler> samplers)
        {
            var state = new AnimationState()
            {
                Clips = clipsBlob,
                StateMachineBlob = stateMachineBlob,
                StateIndex = stateIndex,
                NormalizedTime = 0,
            };
            state.Initialize(ref samplers);
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