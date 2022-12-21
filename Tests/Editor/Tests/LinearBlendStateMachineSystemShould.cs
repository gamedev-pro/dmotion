using System.Linq;
using DMotion.Authoring;
using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(BlendAnimationStatesSystem), typeof(UpdateAnimationStatesSystem))]
    public class LinearBlendStateMachineSystemShould : ECSTestsFixture
    {
        private static readonly float[] thresholds = { 0.0f, 0.5f, 0.8f };

        [Test]
        public void Update_All_Samplers()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, linearBlendState.AnimationStateId);

            var animationState =
                AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, linearBlendState.AnimationStateId);
            var startSamplerIndex =
                ClipSamplerTestUtils.AnimationStateStartSamplerIdToIndex(manager, entity,
                    linearBlendState.AnimationStateId);

            var samplerIndexes = Enumerable.Range(startSamplerIndex, animationState.ClipCount).ToArray();

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
        public void BlendBetweenTwoClips()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, linearBlendState.AnimationStateId);

            //set our blend parameter after the first clip, but closer to the first clip
            var blendParameterValue = thresholds[0] + (thresholds[1] - thresholds[0]) * 0.2f;
            AnimationStateTestUtils.SetBlendParameter(linearBlendState, manager, entity, blendParameterValue);

            UpdateWorld();

            var allSamplers =
                ClipSamplerTestUtils.GetAllSamplersForAnimationState(manager, entity,
                    linearBlendState.AnimationStateId).ToArray();
            
            Assert.Greater(allSamplers[0].Weight, 0);
            Assert.Greater(allSamplers[1].Weight, 0);
            Assert.Zero(allSamplers[2].Weight);
            //expect first clip weight to be greater since we are closer to it
            Assert.Greater(allSamplers[0].Weight, allSamplers[1].Weight);
        }
        
        [Test]
        public void Keep_WeightSum_EqualOne()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, linearBlendState.AnimationStateId);
            AnimationStateTestUtils.SetBlendParameter(linearBlendState, manager, entity, thresholds[0] + 0.1f);

            UpdateWorld();

            var allSamplers =
                ClipSamplerTestUtils.GetAllSamplersForAnimationState(manager, entity,
                    linearBlendState.AnimationStateId);

            var sumWeight = allSamplers.Sum(s => s.Weight);
            Assert.AreEqual(1, sumWeight);
        }

        [Test]
        public void Keep_InactiveSamplerWeight_EqualZero()
        {
            var entity = CreateLinearBlendEntity();
            var linearBlendState = AnimationStateTestUtils.CreateLinearBlendForStateMachine(0, manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, linearBlendState.AnimationStateId);
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
            AnimationStateTestUtils.SetCurrentState(manager, entity, linearBlendState.AnimationStateId);

            var animationState =
                AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, linearBlendState.AnimationStateId);
            var startSamplerIndex =
                ClipSamplerTestUtils.AnimationStateStartSamplerIdToIndex(manager, entity,
                    linearBlendState.AnimationStateId);

            var samplerIndexes = Enumerable.Range(startSamplerIndex, animationState.ClipCount).ToArray();

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
            AnimationStateTestUtils.SetCurrentState(manager, entity, linearBlendState.AnimationStateId);
            var anotherState = AnimationStateTestUtils.CreateSingleClipState(manager, entity);

            var linearBlendStates = manager.GetBuffer<LinearBlendStateMachineState>(entity);
            Assert.AreEqual(1, linearBlendStates.Length);

            const float transitionDuration = 0.2f;
            AnimationStateTestUtils.RequestTransitionTo(manager, entity, anotherState.AnimationStateId,
                transitionDuration);

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
            var stateMachineBuilder = AnimationStateMachineAssetBuilder.New();
            var linearBlendState = stateMachineBuilder.AddState<LinearBlendStateAsset>();
            
            linearBlendState.BlendParameter = stateMachineBuilder.AddParameter<FloatParameterAsset>("blend");
            linearBlendState.BlendClips = new []
            {
                new ClipWithThreshold { Clip = null, Speed = 1, Threshold = thresholds[0] },
                new ClipWithThreshold { Clip = null, Speed = 1, Threshold = thresholds[1] },
                new ClipWithThreshold { Clip = null, Speed = 1, Threshold = thresholds[2] }
            };

            var stateMachineAsset = stateMachineBuilder.Build();

            var stateMachineBlob =
                AnimationStateMachineConversionUtils.CreateStateMachineBlob(stateMachineAsset);

            var clipsBlob = AnimationStateTestUtils.CreateFakeSkeletonClipSetBlob(3);
            
            var entity = manager.CreateStateMachineEntity(stateMachineAsset, stateMachineBlob, clipsBlob, BlobAssetReference<ClipEventsBlob>.Null);
            
            Assert.IsTrue(manager.HasComponent<LinearBlendStateMachineState>(entity));
            Assert.IsTrue(manager.HasComponent<FloatParameter>(entity));
            return entity;
        }
    }
}