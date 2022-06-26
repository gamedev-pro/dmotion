using Unity.Burst;
using Unity.Entities;

namespace DOTSAnimation
{
    [BurstCompile]
    internal partial struct SyncBlendParametersJob : IJobEntity
    {
        public void Execute(ref DynamicBuffer<AnimationState> states, in DynamicBuffer<BlendParameter> blendParameters, in AnimationStateMachine stateMachine)
        {
            for (var i = 0; i < blendParameters.Length; i++)
            {
                var blend = blendParameters[i];
                var stateIndex = blend.StateIndex == stateMachine.CurrentState.StateIndex
                    ? stateMachine.CurrentState.StateIndex
                    : blend.StateIndex == stateMachine.NextState.StateIndex
                        ? stateMachine.NextState.StateIndex
                        : - 1;
                if (stateIndex >= 0)
                {
                    var state = states[stateIndex];
                    state.Blend = blend.Value;
                    states[stateIndex] = state;
                }
            }
        }
    }
}