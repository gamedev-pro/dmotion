using DMotion.Authoring;
using NUnit.Framework;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(AnimationStateMachineSystem))]
    public class StateMachineTransitionsShould : ECSTestsFixture
    {
        [Test]
        public void StartTransition_When_IntParameter_Equals()
        {
            AssertIntTransition(IntConditionComparison.Equal, 1, 1, true);
            AssertIntTransition(IntConditionComparison.Equal, 0, 1, false);
        }
        
        [Test]
        public void StartTransition_When_IntParameter_NotEquals()
        {
            AssertIntTransition(IntConditionComparison.NotEqual, 1, 2, true);
            AssertIntTransition(IntConditionComparison.NotEqual, 1, 1, false);
        }
        
        [Test]
        public void StartTransition_When_IntParameter_Greater()
        {
            AssertIntTransition(IntConditionComparison.Greater, 1, 2, true);
            AssertIntTransition(IntConditionComparison.Greater, 1, 1, false);
            AssertIntTransition(IntConditionComparison.Greater, 1, 0, false);
        }
        
        [Test]
        public void StartTransition_When_IntParameter_GreaterOrEqual()
        {
            AssertIntTransition(IntConditionComparison.GreaterOrEqual, 1, 2, true);
            AssertIntTransition(IntConditionComparison.GreaterOrEqual, 1, 1, true);
            AssertIntTransition(IntConditionComparison.GreaterOrEqual, 1, 0, false);
        }
        
        [Test]
        public void StartTransition_When_IntParameter_Less()
        {
            AssertIntTransition(IntConditionComparison.Less, 1, 0, true);
            AssertIntTransition(IntConditionComparison.Less, 1, 1, false);
            AssertIntTransition(IntConditionComparison.Less, 1, 2, false);
        }
        
        [Test]
        public void StartTransition_When_IntParameter_LessOrEqual()
        {
            AssertIntTransition(IntConditionComparison.LessOrEqual, 1, 0, true);
            AssertIntTransition(IntConditionComparison.LessOrEqual, 1, 1, true);
            AssertIntTransition(IntConditionComparison.LessOrEqual, 1, 2, false);
        }
        

        private void AssertIntTransition(IntConditionComparison comparison, int comparisonValue, int valueToSet, bool expectTransitionToStart)
        {
            CreateStateMachineWithIntTransition(comparison, comparisonValue, out var stateMachineAsset,
                out _, out _,
                out var intParameter);

            var stateMachineBlob =
                AnimationStateMachineConversionUtils.CreateStateMachineBlob(stateMachineAsset,
                    world.UpdateAllocator.ToAllocator);

            var newEntity = manager.CreateStateMachineEntity(stateMachineAsset, stateMachineBlob);
            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, newEntity);

            var intParameterHash = intParameter.Hash;
            manager.SetParameter<IntParameter, int>(newEntity, intParameterHash, valueToSet);
            UpdateWorld();
            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.IsTrue(stateMachine.CurrentState.IsValid);

            var expectedIndex = expectTransitionToStart ? 1 : 0;
            Assert.AreEqual(expectedIndex, stateMachine.CurrentState.StateIndex);
            AnimationStateTestUtils.AssertTransitionRequested(manager, newEntity,
                (byte)stateMachine.CurrentState.AnimationStateId);
        }

        private void CreateStateMachineWithIntTransition(IntConditionComparison comparison, int comparisonValue,
            out StateMachineAsset stateMachineAsset,
            out AnimationStateAsset stateOne, out AnimationStateAsset stateTwo,
            out IntParameterAsset intParameter)
        {
            var stateMachineBuilder = AnimationStateMachineAssetBuilder.New();
            stateOne = stateMachineBuilder.AddState<SingleClipStateAsset>();
            stateTwo = stateMachineBuilder.AddState<SingleClipStateAsset>();
            intParameter = stateMachineBuilder.AddParameter<IntParameterAsset>("intParam");

            var transitionOneTwo = stateMachineBuilder.AddTransition(stateOne, stateTwo);
            stateMachineBuilder.AddIntCondition(transitionOneTwo, intParameter,
                comparison, comparisonValue);

            stateMachineAsset = stateMachineBuilder.Build();
        }
    }
}