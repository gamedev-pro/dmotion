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