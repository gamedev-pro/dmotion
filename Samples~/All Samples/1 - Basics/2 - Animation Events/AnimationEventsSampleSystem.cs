using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Samples.AnimationEvents
{
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct AnimationEventsSampleSystem : ISystem
    {
        private static readonly int FootstepEventHash = StateMachineParameterUtils.GetHashCode("EV_Footstep");

        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<AnimationEventsSample>();
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var raisedEvents in SystemAPI.Query<DynamicBuffer<RaisedAnimationEvent>>())
            {
                if (raisedEvents.WasEventRaised(FootstepEventHash))
                {
                    Debug.Log("Footstep raised!");
                }
            }
        }
    }
}