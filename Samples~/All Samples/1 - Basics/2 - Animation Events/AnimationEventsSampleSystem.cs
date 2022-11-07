using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.AnimationEvents
{
    [DisableAutoCreation]
    public partial class AnimationEventsSampleSystem : SystemBase
    {
        private static readonly int FootstepEventHash = StateMachineParameterUtils.GetHashCode("EV_Footstep");
        protected override void OnUpdate()
        {
            Entities.ForEach((in DynamicBuffer<RaisedAnimationEvent> raisedEvents) =>
            {
                if (raisedEvents.WasEventRaised(FootstepEventHash))
                {
                    Debug.Log("Footstep raised!");
                }
            }).Schedule();
        }
    }
}