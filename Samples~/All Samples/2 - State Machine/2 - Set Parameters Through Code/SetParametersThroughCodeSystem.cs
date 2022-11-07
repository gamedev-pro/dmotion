using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.StateMachine
{
    [DisableAutoCreation]
    public partial class SetParametersThroughCodeSystem : SystemBase
    {
        private static readonly int IsRunningHash = StateMachineParameterUtils.GetHashCode("IsRunning");
        protected override void OnUpdate()
        {
            var toggleIsRunning = Input.GetKeyDown(KeyCode.Space);
            
            Entities.ForEach((ref DynamicBuffer<BoolParameter> boolParameters) =>
            {
                if (toggleIsRunning)
                {
                    var currentValue = boolParameters.GetValue<BoolParameter, bool>(IsRunningHash);
                    boolParameters.SetValue(IsRunningHash, !currentValue);
                }
            }).Schedule();
        }
    }
}