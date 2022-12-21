using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.StateMachine
{
    [RequireMatchingQueriesForUpdate]
    public partial struct SetParametersThroughCodeSystem : ISystem
    {
        private static readonly int IsRunningHash = StateMachineParameterUtils.GetHashCode("IsRunning");
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<SetParametersThroughCodeSample>();
        }
        public void OnDestroy(ref SystemState state)
        {
        }
        
        public void OnUpdate(ref SystemState state)
        {
            var toggleIsRunning = Input.GetKeyDown(KeyCode.Space);

            foreach (var boolParameters in SystemAPI.Query<DynamicBuffer<BoolParameter>>())
            {
                if (toggleIsRunning)
                {
                    var currentValue = boolParameters.GetValue<BoolParameter, bool>(IsRunningHash);
                    boolParameters.SetValue(IsRunningHash, !currentValue);
                }
            }
        }
    }
}