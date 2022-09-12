using DMotion.Authoring;
using NUnit.Framework;
using Unity.Collections;
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
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();
            ECSTestUtils.AssertSystemQueries<AnimationStateMachineSystem>(world);
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

        // [Test]
        // public void UpdateActiveSamplers()
        // {
        //     var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
        //     Assert.IsTrue(manager.HasComponent<ClipSampler>(newEntity));
        //     UpdateWorld();
        //
        //     var samplersBefore = manager.GetBuffer<ClipSampler>(newEntity).ToNativeArray(Allocator.Temp);
        //     UpdateWorld();
        //     var samplersAfter = manager.GetBuffer<ClipSampler>(newEntity).ToNativeArray(Allocator.Temp);
        //
        //     Assert.NotZero(samplersBefore.Length);
        //     Assert.AreEqual(samplersBefore.Length, samplersAfter.Length);
        //     for (var i = 0; i < samplersBefore.Length; i++)
        //     {
        //         var before = samplersBefore[i];
        //         var after = samplersAfter[i];
        //         Assert.Greater(after.Time, before.Time);
        //     }
        // }

        [Test]
        public void StartTransition_From_BoolParameter()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();

            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            var cachedCurrentState = stateMachine.CurrentState;

            manager.SetBoolParameter(newEntity, 0, true);

            UpdateWorld();

            stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            
            Assert.AreNotEqual(stateMachine.CurrentState, cachedCurrentState);
        }

        // [Test]
        // public void CompleteTransition_After_TransitionDuration()
        // {
        //     var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
        //     manager.SetBoolParameter(newEntity, 0, true);
        //     UpdateWorld();
        //
        //     var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
        //     var cachedNextState = stateMachine.NextState;
        //     Assert.AreNotEqual(cachedNextState, StateMachineStateRef.Null);
        //
        //     UpdateWorld(stateMachine.CurrentTransitionDuration * 1.5f);
        //     UpdateWorld();
        //
        //     stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
        //     Assert.AreEqual(stateMachine.NextState, StateMachineStateRef.Null);
        //     Assert.AreEqual(stateMachine.CurrentState.StateIndex, cachedNextState.StateIndex);
        //     Assert.AreEqual(stateMachine.CurrentState.Type, cachedNextState.Type);
        // }
    }
}