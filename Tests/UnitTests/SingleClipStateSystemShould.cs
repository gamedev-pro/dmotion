using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(BlendAnimationStatesSystem), typeof(UpdateAnimationStatesSystem))]
    public class SingleClipStateSystemShould : ECSTestBase
    {
        [Test]
        public void UpdateSamplers()
        {
            var entity = CreateSingleClipStateEntity();
            var singleClip = AnimationStateTestUtils.CreateSingleClipState(manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, singleClip.AnimationStateId);

            var sampler = ClipSamplerTestUtils.GetFirstSamplerForAnimationState(manager, entity, singleClip.AnimationStateId);
            Assert.AreEqual(0, sampler.Weight);
            Assert.AreEqual(0, sampler.Time);
            Assert.AreEqual(0, sampler.PreviousTime);

            UpdateWorld();

            sampler = ClipSamplerTestUtils.GetFirstSamplerForAnimationState(manager, entity, singleClip.AnimationStateId);
            Assert.Greater(sampler.Weight, 0);
            Assert.Greater(sampler.Time, 0);
            Assert.AreEqual(0, sampler.PreviousTime);

            var prevTime = sampler.Time;

            UpdateWorld();

            sampler = ClipSamplerTestUtils.GetFirstSamplerForAnimationState(manager, entity, singleClip.AnimationStateId);
            Assert.Greater(sampler.Time, prevTime);
            Assert.AreEqual(prevTime, sampler.PreviousTime);
        }

        [Test]
        public void LoopToClipTime()
        {
             var entity = CreateSingleClipStateEntity();
             var singleClip = AnimationStateTestUtils.CreateSingleClipState(manager, entity, speed: 1, loop: true);
             AnimationStateTestUtils.SetCurrentState(manager, entity, singleClip.AnimationStateId);
             
             UpdateWorld();
             
             var sampler = ClipSamplerTestUtils.GetFirstSamplerForAnimationState(manager, entity, singleClip.AnimationStateId);
             Assert.Greater(sampler.Weight, 0);
             Assert.Greater(sampler.Time, 0);
             Assert.AreEqual(0, sampler.PreviousTime);
        
             var prevTime = sampler.Time;
             
             UpdateWorld(sampler.Clip.duration - prevTime * 0.5f);
             
             sampler = ClipSamplerTestUtils.GetFirstSamplerForAnimationState(manager, entity, singleClip.AnimationStateId);
             //clip time should have looped
             Assert.Less(sampler.Time, prevTime);
             Assert.AreEqual(prevTime, sampler.PreviousTime);           
        }

        [Test]
        public void CleanupStates()
        {
            var entity = CreateSingleClipStateEntity();
            var s1 = AnimationStateTestUtils.CreateSingleClipState(manager, entity);
            var s2 = AnimationStateTestUtils.CreateSingleClipState(manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, s1.AnimationStateId);

            var singleClips = manager.GetBuffer<SingleClipState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.AreEqual(2, singleClips.Length);
            Assert.AreEqual(2, samplers.Length);

            const float transitionDuration = 0.2f;
            AnimationStateTestUtils.RequestTransitionTo(manager, entity, s2.AnimationStateId, transitionDuration);
            
            UpdateWorld();
            
            singleClips = manager.GetBuffer<SingleClipState>(entity);
            samplers = manager.GetBuffer<ClipSampler>(entity);
            //We should still have both clips since we're transitioning
            Assert.AreEqual(2, singleClips.Length);
            Assert.AreEqual(2, samplers.Length);
            
            UpdateWorld(transitionDuration);
            
            //Should have cleanup s1 after transition
            singleClips = manager.GetBuffer<SingleClipState>(entity);
            samplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.AreEqual(1, singleClips.Length);
            Assert.AreEqual(s2.AnimationStateId, singleClips[0].AnimationStateId);
            Assert.AreEqual(1, samplers.Length);
            Assert.AreEqual(1, samplers[0].Weight);
        }

        private Entity CreateSingleClipStateEntity()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            manager.AddBuffer<SingleClipState>(entity);
            return entity;
        }
    }
}