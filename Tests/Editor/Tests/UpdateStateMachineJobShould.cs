using DMotion.Authoring;
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(AnimationStateMachineSystem))]
    public class UpdateStateMachineJobShould : ECSTestsFixture
    {
        [Test]
        public void Run_With_Valid_Queries()
        {
            CreateStateMachineEntity(out _, out _, out _);
            UpdateWorld();
            ECSTestUtils.AssertSystemQueries<AnimationStateMachineSystem>(world);
        }

        [Test]
        public void Be_Active_When_AnimationStateNotValid()
        {
            CreateStateMachineEntity(out _, out _, out var newEntity);
            AnimationStateTestUtils.SetInvalidCurrentState(manager, newEntity);
            AnimationStateTestUtils.AssertCurrentStateInvalid(manager, newEntity);
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, newEntity);
            Assert.IsTrue(shouldBeActive, "Expected state machine should be active if current animation state is invalid");
        }
        
        [Test]
        public void Be_Active_When_CurrentStateMachineState_Is_CurrentAnimationState()
        {
            CreateStateMachineEntity(out _, out _, out var entity);
            UpdateWorld();
            var currentState = StateMachineTestUtils.GetCurrentState(manager, entity);
            Assert.IsTrue(currentState.IsValid);
            AnimationStateTestUtils.SetCurrentState(manager, entity, (byte)currentState.AnimationStateId);
            
            AnimationStateTestUtils.AssertCurrentState(manager, entity, (byte)currentState.AnimationStateId);
            
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, entity);
            Assert.IsTrue(shouldBeActive, "Expected state machine should be active if Current State Machine State is current Animation State");
        }
        
        [Test]
        public void Be_Active_When_CurrentStateMachineState_Is_AnimationStateTransition()
        {
            CreateStateMachineEntity(out _, out _, out var entity);
            UpdateWorld();
            
            var singleState = AnimationStateTestUtils.CreateSingleClipState(manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, singleState.AnimationStateId);
            
            var currentState = StateMachineTestUtils.GetCurrentState(manager, entity);
            Assert.IsTrue(currentState.IsValid);
        
            AnimationStateTestUtils.SetAnimationStateTransition(manager, entity, (byte)currentState.AnimationStateId);
            AnimationStateTestUtils.AssertOnGoingTransition(manager, entity, (byte)currentState.AnimationStateId);
            
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, entity);
            Assert.IsTrue(shouldBeActive, "Expected state machine should be active if Current State Machine State is Animation State Transition");
        }
        
        [Test]
        public void NotInitialize_If_NotPlaying()
        {
            CreateStateMachineEntity(out _, out _, out var newEntity);
            var currentState = StateMachineTestUtils.GetCurrentState(manager, newEntity);
            Assert.AreEqual(currentState, StateMachineStateRef.Null);
            
            //Set a random current animationState that is not us
            var otherState =
                AnimationStateTestUtils.NewAnimationStateFromEntity(manager, newEntity, default(ClipSampler));
            AnimationStateTestUtils.SetCurrentState(manager, newEntity, otherState.Id);
        
            var shouldBeActive = StateMachineTestUtils.ShouldStateMachineBeActive(manager, newEntity);
            Assert.IsFalse(shouldBeActive, "Expected state machine not to be active");
            UpdateWorld();
        
            //We shouldn't have initialized
            currentState = StateMachineTestUtils.GetCurrentState(manager, newEntity);
            Assert.AreEqual(currentState, StateMachineStateRef.Null, "Expected state machine not to have initialized");
        }
        
        [Test]
        public void Initialize_When_Necessary()
        {
            CreateStateMachineEntity(out _, out _, out var newEntity);
            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreEqual(stateMachine.CurrentState, StateMachineStateRef.Null);
        
            UpdateWorld();
        
            stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreNotEqual(stateMachine.CurrentState, StateMachineStateRef.Null);
        }
        
        private void CreateStateMachineEntity(
            out StateMachineAsset stateMachineAsset,
            out BlobAssetReference<StateMachineBlob> stateMachineBlob,
            out Entity entity)
        {
            var stateMachineBuilder = AnimationStateMachineAssetBuilder.New();
            stateMachineBuilder.AddState<SingleClipStateAsset>();

            stateMachineAsset = stateMachineBuilder.Build();

            stateMachineBlob =
                AnimationStateMachineConversionUtils.CreateStateMachineBlob(stateMachineAsset,
                    world.UpdateAllocator.ToAllocator);

            entity = manager.CreateStateMachineEntity(stateMachineAsset, stateMachineBlob);
        }
    }
    
}