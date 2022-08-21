using DMotion.Authoring;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Tests
{
    public class UpdateStateMachineJobShould : ECSTestsFixture
    {
        [SerializeField] private AnimationStateMachineAuthoring stateMachinePrefab;
        private Entity stateMachineEntityPrefab;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Assert.IsNotNull(stateMachinePrefab, "Missing prefab");
            Assert.IsNotNull(stateMachinePrefab.StateMachineAsset, "Missing StateMachineAsset");
            var convertToEntitySystem = world.GetOrCreateSystem<ConvertToEntitySystem>();
            stateMachineEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(
                stateMachinePrefab.gameObject,
                new GameObjectConversionSettings(world, GameObjectConversionUtility.ConversionFlags.AssignName,
                    convertToEntitySystem.BlobAssetStore));

            Assert.IsTrue(manager.HasComponent<Prefab>(stateMachineEntityPrefab));

            world.CreateSystem<AnimationStateMachineSystem>();
        }

        private Entity InstantiateStateMachineEntity()
        {
            var newEntity = manager.Instantiate(stateMachineEntityPrefab);
            Assert.IsTrue(manager.HasComponent<AnimationStateMachine>(newEntity));
            return newEntity;
        }

        private void SetBoolParameter(Entity entity, int index, bool newValue)
        {
            Assert.IsTrue(manager.HasComponent<BoolParameter>(entity));
            var boolParameters = manager.GetBuffer<BoolParameter>(entity);
            Assert.IsTrue(boolParameters.Length > 0);
            var parameter = boolParameters[index];
            parameter.Value = newValue;
            boolParameters[index] = parameter;
        }

        [Test]
        public void Initialize_When_Necessary()
        {
            var newEntity = InstantiateStateMachineEntity();
            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreEqual(stateMachine.CurrentState, AnimationState.Null);

            UpdateWorld();

            stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreNotEqual(stateMachine.CurrentState, AnimationState.Null);
        }

        [Test]
        public void UpdateActiveSamplers()
        {
            var newEntity = InstantiateStateMachineEntity();
            Assert.IsTrue(manager.HasComponent<ClipSampler>(newEntity));
            UpdateWorld();

            var samplersBefore = manager.GetBuffer<ClipSampler>(newEntity).ToNativeArray(Allocator.Temp);
            UpdateWorld();
            var samplersAfter = manager.GetBuffer<ClipSampler>(newEntity).ToNativeArray(Allocator.Temp);

            Assert.AreEqual(samplersBefore.Length, samplersAfter.Length);
            for (int i = 0; i < samplersBefore.Length; i++)
            {
                var before = samplersBefore[i];
                var after = samplersAfter[i];
                Assert.Greater(after.Time, before.Time);
            }
        }

        [Test]
        public void StartTransition_From_BoolParameter()
        {
            var newEntity = InstantiateStateMachineEntity();
            UpdateWorld();

            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreEqual(stateMachine.NextState, AnimationState.Null);

            SetBoolParameter(newEntity, 0, true);

            UpdateWorld();

            stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);

            Assert.AreNotEqual(stateMachine.NextState, AnimationState.Null);
        }

        [Test]
        public void CompleteTransition_After_TransitionDuration()
        {
            var newEntity = InstantiateStateMachineEntity();
            SetBoolParameter(newEntity, 0, true);
            UpdateWorld();

            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            var cachedNextState = stateMachine.NextState;
            Assert.AreNotEqual(cachedNextState, AnimationState.Null);

            UpdateWorld(stateMachine.CurrentTransitionDuration*1.5f);
            UpdateWorld();
            
            stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            Assert.AreEqual(stateMachine.NextState, AnimationState.Null);
            Assert.AreEqual(stateMachine.CurrentState.StateIndex, cachedNextState.StateIndex);
            Assert.AreEqual(stateMachine.CurrentState.Type, cachedNextState.Type);
        }
    }
}