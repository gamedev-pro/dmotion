using NUnit.Framework;
using Unity.Entities;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(PlayablesSystem), typeof(UpdateAnimationStatesSystem))]
    public class SingleClipStateSystemShould : ECSTestsFixture
    {
        [Test]
        public void Run_With_Valid_Queries()
        {
            CreateSingleClipStateEntity();
            UpdateWorld();
            ECSTestUtils.AssertSystemQueries<UpdateAnimationStatesSystem>(world);
            ECSTestUtils.AssertSystemQueries<PlayablesSystem>(world);
        }

        [Test]
        public void UpdateSamplers()
        {
            var entity = CreateSingleClipStateEntity();
            var singleClip = AnimationStateTestUtils.CreateSingleClipState(manager, entity);
            PlayableTestUtils.SetCurrentState(manager, entity, singleClip.PlayableId);

            var sampler = ClipSamplerTestUtils.GetFirstSamplerForPlayable(manager, entity, singleClip.PlayableId);
            Assert.AreEqual(0, sampler.Weight);
            Assert.AreEqual(0, sampler.Time);
            Assert.AreEqual(0, sampler.PreviousTime);

            UpdateWorld();

            sampler = ClipSamplerTestUtils.GetFirstSamplerForPlayable(manager, entity, singleClip.PlayableId);
            Assert.Greater(sampler.Weight, 0);
            Assert.Greater(sampler.Time, 0);
            Assert.AreEqual(0, sampler.PreviousTime);

            var prevTime = sampler.Time;

            UpdateWorld();

            sampler = ClipSamplerTestUtils.GetFirstSamplerForPlayable(manager, entity, singleClip.PlayableId);
            Assert.Greater(sampler.Time, prevTime);
            Assert.AreEqual(prevTime, sampler.PreviousTime);
        }

        [Test]
        public void LoopToClipTime()
        {
             var entity = CreateSingleClipStateEntity();
             var singleClip = AnimationStateTestUtils.CreateSingleClipState(manager, entity, speed: 1, loop: true);
             PlayableTestUtils.SetCurrentState(manager, entity, singleClip.PlayableId);
             
             UpdateWorld();
             
             var sampler = ClipSamplerTestUtils.GetFirstSamplerForPlayable(manager, entity, singleClip.PlayableId);
             Assert.Greater(sampler.Weight, 0);
             Assert.Greater(sampler.Time, 0);
             Assert.AreEqual(0, sampler.PreviousTime);
        
             var prevTime = sampler.Time;
             
             UpdateWorld(sampler.Clip.duration - prevTime * 0.5f);
             
             sampler = ClipSamplerTestUtils.GetFirstSamplerForPlayable(manager, entity, singleClip.PlayableId);
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
            PlayableTestUtils.SetCurrentState(manager, entity, s1.PlayableId);

            var singleClips = manager.GetBuffer<SingleClipState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.AreEqual(2, singleClips.Length);
            Assert.AreEqual(2, samplers.Length);

            const float transitionDuration = 0.2f;
            PlayableTestUtils.TransitionTo(manager, entity, s2.PlayableId, transitionDuration);
            
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
            Assert.AreEqual(s2.PlayableId, singleClips[0].PlayableId);
            Assert.AreEqual(1, samplers.Length);
            Assert.AreEqual(1, samplers[0].Weight);
        }

        private Entity CreateSingleClipStateEntity()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            manager.AddBuffer<SingleClipState>(entity);
            return entity;
        }
    }
}