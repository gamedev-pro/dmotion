using System.Collections.Generic;
using System.Linq;
using System.Management.Instrumentation;
using DMotion.Authoring;
using NUnit.Framework;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(PlayablesSystem), typeof(UpdateAnimationStatesSystem))]
    public class LinearBlendStateMachineSystemShould : ECSTestsFixture
    {
        [SerializeField, ConvertGameObjectPrefab(nameof(stateMachineEntityPrefab))]
        private AnimationStateMachineAuthoring stateMachinePrefab;

        private Entity stateMachineEntityPrefab;

        [Test]
        public void Run_With_Valid_Queries()
        {
            CreateLinearBlendEntity();
            UpdateWorld();
            ECSTestUtils.AssertSystemQueries<UpdateAnimationStatesSystem>(world);
            ECSTestUtils.AssertSystemQueries<PlayablesSystem>(world);
        }

        [Test]
        public void Update_All_Samplers()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            PlayableTestUtils.SetCurrentState(manager, entity, linearBlendState.PlayableId);

            var playable = PlayableTestUtils.GetPlayableFromEntity(manager, entity, linearBlendState.PlayableId);
            var startSamplerIndex =
                ClipSamplerTestUtils.PlayableStartSamplerIdToIndex(manager, entity, linearBlendState.PlayableId);

            var samplerIndexes = Enumerable.Range(startSamplerIndex, playable.ClipCount).ToArray();

            //Assert everything is zero
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            foreach (var i in samplerIndexes)
            {
                var sampler = samplers[i];
                Assert.AreEqual(0, sampler.Weight);
                Assert.AreEqual(0, sampler.Time);
                Assert.AreEqual(0, sampler.PreviousTime);
            }

            UpdateWorld();

            samplers = manager.GetBuffer<ClipSampler>(entity);
            foreach (var i in samplerIndexes)
            {
                var sampler = samplers[i];
                Assert.Greater(sampler.Time, 0);
                Assert.AreEqual(0, sampler.PreviousTime);
            }
        }

        [Test]
        public void Keep_WeightSum_EqualOne()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            PlayableTestUtils.SetCurrentState(manager, entity, linearBlendState.PlayableId);
            AnimationStateTestUtils.SetBlendParameter(linearBlendState, manager, entity, 0.1f);

            UpdateWorld();

            var allSamplers =
                ClipSamplerTestUtils.GetAllSamplersForPlayable(manager, entity, linearBlendState.PlayableId);

            var sumWeight = allSamplers.Sum(s => s.Weight);
            Assert.AreEqual(1, sumWeight);
        }

        [Test]
        public void Keep_InactiveSamplerWeight_EqualZero()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            PlayableTestUtils.SetCurrentState(manager, entity, linearBlendState.PlayableId);
            AnimationStateTestUtils.SetBlendParameter(linearBlendState, manager, entity, 0.1f);

            UpdateWorld();

            AnimationStateTestUtils.FindActiveSamplerIndexesForLinearBlend(linearBlendState, manager, entity,
                out var firstClipIndex, out var secondClipIndex);

            var samplers = manager.GetBuffer<ClipSampler>(entity).AsNativeArray().ToArray();
            var inactiveSamplers = samplers.TakeWhile((e, i) => i != firstClipIndex && i != secondClipIndex);

            var sumWeight = inactiveSamplers.Sum(s => s.Weight);
            Assert.AreEqual(0, sumWeight);
        }

        [Test]
        public void LoopToClipTime()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            PlayableTestUtils.SetCurrentState(manager, entity, linearBlendState.PlayableId);

            var playable = PlayableTestUtils.GetPlayableFromEntity(manager, entity, linearBlendState.PlayableId);
            var startSamplerIndex =
                ClipSamplerTestUtils.PlayableStartSamplerIdToIndex(manager, entity, linearBlendState.PlayableId);

            var samplerIndexes = Enumerable.Range(startSamplerIndex, playable.ClipCount).ToArray();
            
            foreach (var i in samplerIndexes)
            {
                var samplers = manager.GetBuffer<ClipSampler>(entity);
                var sampler = samplers[i];
                
                //We need to set Time = duration to guarantee clip will loop on next frame
                sampler.Time = sampler.Clip.duration;
                sampler.PreviousTime = sampler.Time - 0.1f;
                samplers[i] = sampler;
                
                UpdateWorld();

                var updatedSamplers = manager.GetBuffer<ClipSampler>(entity);
                var updatedSampler = updatedSamplers[i];
                //Because previous Time = duration, previous time will loop to 0
                Assert.AreEqual(0, updatedSampler.PreviousTime);
                Assert.Greater(updatedSampler.Time, updatedSampler.PreviousTime);
                //clip time should have looped
                Assert.Less(updatedSampler.Time, sampler.Time);
            }
        }

        [Test]
        public void CleanupStates()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            PlayableTestUtils.SetCurrentState(manager, entity, linearBlendState.PlayableId);
            var anotherState = AnimationStateTestUtils.CreateSingleClipState(manager, entity);

            var linearBlendStates = manager.GetBuffer<LinearBlendStateMachineState>(entity);
            Assert.AreEqual(1, linearBlendStates.Length);

            const float transitionDuration = 0.2f;
            PlayableTestUtils.TransitionTo(manager, entity, anotherState.PlayableId, transitionDuration);

            UpdateWorld();

            linearBlendStates = manager.GetBuffer<LinearBlendStateMachineState>(entity);
            //We should still have both clips since we're transitioning
            Assert.AreEqual(1, linearBlendStates.Length);

            UpdateWorld(transitionDuration);

            //Should have cleanup the linear blend state
            linearBlendStates = manager.GetBuffer<LinearBlendStateMachineState>(entity);
            Assert.Zero(linearBlendStates.Length);
        }

        private Entity CreateLinearBlendEntity()
        {
            var entity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            Assert.IsTrue(manager.HasComponent<LinearBlendStateMachineState>(entity));
            Assert.IsTrue(manager.HasComponent<BlendParameter>(entity));
            return entity;
        }
    }
}