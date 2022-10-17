using DMotion.Authoring;
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(AnimationStateMachineSystem))]
    public class UpdateStateMachineJobShould : ECSTestsFixture
    {
        [SerializeField, ConvertGameObjectPrefab(nameof(stateMachineEntityPrefab))]
        private AnimationStateMachineAuthoring stateMachinePrefab;

        private Entity stateMachineEntityPrefab;

        [Test]
        public void Run_With_Valid_Queries()
        {
            manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();
            ECSTestUtils.AssertSystemQueries<AnimationStateMachineSystem>(world);
        }

        [Test]
        public void Be_Active_When_AnimationStateNotValid()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            AnimationStateTestUtils.SetInvalidCurrentState(manager, newEntity);
            AnimationStateTestUtils.AssertCurrentStateInvalid(manager, newEntity);
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, newEntity);
            Assert.IsTrue(shouldBeActive, "Expected state machine should be active if current animation state is invalid");
        }
        
        [Test]
        public void Be_Active_When_StateMachineTransitionIsRequested()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            var singleState = AnimationStateTestUtils.CreateSingleClipState(manager, newEntity);
            AnimationStateTestUtils.SetCurrentState(manager, newEntity, singleState.AnimationStateId);
            UpdateWorld();
            AnimationStateTestUtils.AssertCurrentState(manager, newEntity, singleState.AnimationStateId);
            
            StateMachineTestUtils.RequestTransitionToStateMachine(manager, newEntity);
            
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, newEntity);
            Assert.IsTrue(shouldBeActive, "Expected state machine should be active if state machine transition is requested");
        }
        
        [Test]
        public void Be_Active_When_CurrentStateMachineState_Is_CurrentAnimationState()
        {
            var entity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();
            var currentState = StateMachineTestUtils.GetCurrentState(manager, entity);
            Assert.IsTrue(currentState.IsValid);
            AnimationStateTestUtils.SetCurrentState(manager, entity, (byte)currentState.AnimationStateId);
            
            manager.SetComponentData(entity, AnimationStateMachineTransitionRequest.Null);
            
            AnimationStateTestUtils.AssertCurrentState(manager, entity, (byte)currentState.AnimationStateId);
            
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, entity);
            Assert.IsTrue(shouldBeActive, "Expected state machine should be active if Current State Machine State is current Animation State");
        }
        [Test]
        public void Be_Active_When_CurrentStateMachineState_Is_AnimationStateTransition()
        {
            var entity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();
            
            var singleState = AnimationStateTestUtils.CreateSingleClipState(manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, singleState.AnimationStateId);
            
            var currentState = StateMachineTestUtils.GetCurrentState(manager, entity);
            Assert.IsTrue(currentState.IsValid);

            AnimationStateTestUtils.SetAnimationStateTransition(manager, entity, (byte)currentState.AnimationStateId);
            AnimationStateTestUtils.AssertOnGoingTransition(manager, entity, (byte)currentState.AnimationStateId);
            
            manager.SetComponentData(entity, AnimationStateMachineTransitionRequest.Null);
            
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, entity);
            Assert.IsTrue(shouldBeActive, "Expected state machine should be active if Current State Machine State is Animation State Transition");
        }
        
        [Test]
        public void NotInitialize_If_NotPlaying()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreEqual(stateMachine.CurrentState, StateMachineStateRef.Null);
            
            //Set a random current animationState that is not us
            manager.SetComponentData(newEntity, new AnimationCurrentState
            {
                AnimationStateId = 1
            });

            UpdateWorld();

            //We shouldn't have initialized
            stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreEqual(stateMachine.CurrentState, StateMachineStateRef.Null);
        }
        
        [Test]
        public void Initialize_When_Necessary()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreEqual(stateMachine.CurrentState, StateMachineStateRef.Null);

            UpdateWorld();

            stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreNotEqual(stateMachine.CurrentState, StateMachineStateRef.Null);
        }

        [Test]
        public void StartTransition_From_BoolParameter()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, newEntity);
            manager.SetBoolParameter(newEntity, 0, true);
            UpdateWorld();

            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.IsTrue(stateMachine.CurrentState.IsValid);
            AnimationStateTestUtils.AssertTransitionRequested(manager, newEntity, (byte)stateMachine.CurrentState.AnimationStateId);
        }
        
        [Test]
        public void StartTransition_From_IntParameter()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, newEntity);
            manager.SetIntParameter(newEntity, 0, 1);
            UpdateWorld();

            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.IsTrue(stateMachine.CurrentState.IsValid);
            AnimationStateTestUtils.AssertTransitionRequested(manager, newEntity, (byte)stateMachine.CurrentState.AnimationStateId);
        }
    }
}