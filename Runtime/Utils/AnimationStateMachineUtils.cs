using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

namespace DOTSAnimation
{
    public static partial class AnimationStateMachineUtils
    {
        public static void PlayOneShot(this AnimationStateRef stateRef, ref AnimationStateMachine fms)
        {
            fms.RequestedNextState = new AnimationStateMachine.StateRef() { StateIndex = stateRef.Index, IsOneShot = true };
        }
        
        public static void RaiseExceptionIfNotValid(in AnimationStateMachine stateMachine, in DynamicBuffer<AnimationState> states)
        {
            Assert.IsTrue(stateMachine.CurrentState.IsValid);
            Assert.IsTrue(states.IsValidIndex(stateMachine.CurrentState.StateIndex));
        }
    }
}