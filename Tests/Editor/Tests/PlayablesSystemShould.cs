using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using AssertionException = UnityEngine.Assertions.AssertionException;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(PlayablesSystem))]
    public class PlayablesSystemShould : ECSTestsFixture
    {
        [Test]
        public void Create_New_Playable()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            Assert.GreaterOrEqual(playable.Id, 0);
        }

        [Test]
        public void Reject_Playable_With_NoClips()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            Assert.Throws<AssertionException>(() =>
            {
                PlayableTestUtils.NewPlayableFromEntity(manager, entity,
                    new NativeArray<ClipSampler>(0, Allocator.Temp));
            });
        }

        [Test]
        public void Update_Playables()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            Assert.Zero(playable.Time);
            Assert.NotZero(playable.Speed);
            
            //Force playable to not be cleaned-up
            playable.Weight = 1;
            PlayableTestUtils.SetPlayable(manager, entity, playable);
            UpdateWorld();
            playable = PlayableTestUtils.GetPlayableFromEntity(manager, entity, playable.Id);
            Assert.Greater(playable.Time, 0);
        }
        
        [Test]
        public void InstantlyTransition_If_CurrentState_IsNotValid()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            var currentState = manager.GetComponentData<PlayableCurrentState>(entity);
            Assert.IsFalse(currentState.IsValid);
            
            PlayableTestUtils.TransitionTo(manager, entity, playable.Id);
            
            //Force playable to not be cleaned-up
            playable.Weight = 1;
            PlayableTestUtils.SetPlayable(manager, entity, playable);
            UpdateWorld();
            playable = PlayableTestUtils.GetPlayableFromEntity(manager, entity, playable.Id);
            Assert.Greater(playable.Time, 0);
        }
        

        [Test]
        public void StartTransition_From_Request()
        {
            const float transitionDuration = 0.1f;
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.AssertNoOnGoingTransition(manager, entity);
            Assert.IsFalse(manager.GetComponentData<PlayableCurrentState>(entity).IsValid);

            manager.SetComponentData(entity, new PlayableTransitionRequest
            {
                PlayableId = (sbyte)playable.Id,
                TransitionDuration = transitionDuration
            });

            UpdateWorld();
            PlayableTestUtils.AssertOnGoingTransition(manager, entity, playable.Id);
        }

        [Test]
        public void StartTransition_From_Request_Even_When_Transitioning()
        {
            const float transitionDuration = 0.1f;
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.AssertNoOnGoingTransition(manager, entity);
            Assert.IsFalse(manager.GetComponentData<PlayableCurrentState>(entity).IsValid);

            manager.SetComponentData(entity, new PlayableTransitionRequest
            {
                PlayableId = (sbyte)playable.Id,
                TransitionDuration = transitionDuration
            });

            UpdateWorld();
            PlayableTestUtils.AssertOnGoingTransition(manager, entity, playable.Id);

            var secondPlayable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));

            manager.SetComponentData(entity, new PlayableTransitionRequest
            {
                PlayableId = (sbyte)secondPlayable.Id
            });

            UpdateWorld();
            PlayableTestUtils.AssertOnGoingTransition(manager, entity, secondPlayable.Id);
        }

        [Test]
        public void EndTransition_After_TransitionDuration()
        {
            const float transitionDuration = 0.1f;
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.SetCurrentState(manager, entity, 0);
            PlayableTestUtils.AssertNoOnGoingTransition(manager, entity);
            
            PlayableTestUtils.AssertCurrentState(manager, entity, 0);
            PlayableTestUtils.TransitionTo(manager, entity, playable.Id, transitionDuration);

            //update a couple of times and make sure we're still transitioning
            for (var i = 0; i < 3; i++)
            {
                UpdateWorld();
                PlayableTestUtils.AssertOnGoingTransition(manager, entity, playable.Id);
            }

            // make sure we complete the transition
            UpdateWorld(transitionDuration);
            PlayableTestUtils.AssertNoOnGoingTransition(manager, entity);
            PlayableTestUtils.AssertCurrentState(manager, entity, playable.Id);
        }

        [Test]
        public void IncreaseWeight_During_Transition()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var p1 = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            var p2 = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));

            PlayableTestUtils.AssertNoOnGoingTransition(manager, entity);
            PlayableTestUtils.SetCurrentState(manager, entity, p1.Id);
            PlayableTestUtils.TransitionTo(manager, entity, p2.Id);

            var weight = p2.Weight;
            Assert.Zero(weight);
            for (var i = 0; i < 3; i++)
            {
                UpdateWorld();
                PlayableTestUtils.AssertOnGoingTransition(manager, entity, p2.Id);
                p2 = PlayableTestUtils.GetPlayableFromEntity(manager, entity, p2.Id);
                Assert.Greater(p2.Weight, weight);
                weight = p2.Weight;
            }
        }

        [Test]
        public void Cleanup_Playable_With_Zero_Weight()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            
            var playables = manager.GetBuffer<PlayableState>(entity);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.AreEqual(3, playables.Length);
            Assert.AreEqual(3, clipSamplers.Length);

            UpdateWorld();

            playables = manager.GetBuffer<PlayableState>(entity);
            clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.Zero(playables.Length);
            Assert.Zero(clipSamplers.Length);
        }

        [Test]
        public void NotCleanup_Playable_When_TransitionActive()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            
            var playables = manager.GetBuffer<PlayableState>(entity);
            Assert.AreEqual(3, playables.Length);

            //We need to assure weights of other clips are not zero, PlayablesSystem asserts this, since 
            for (var i = 1; i < playables.Length; i++)
            {
                var p = playables[i];
                p.Weight = 0.5f;
                playables[i] = p;
            }
            
            manager.SetComponentData(entity, new PlayableTransitionRequest
            {
                PlayableId = (sbyte)playable.Id,
                TransitionDuration = 0.1f
            });
            
            UpdateWorld();

            playables = manager.GetBuffer<PlayableState>(entity);
            Assert.AreEqual(1, playables.Length);
            Assert.AreEqual(playable.Id, playables[0].Id);
        }

        [Test]
        public void Keep_SumWeights_Equal_One()
        {
            var entity = PlayableTestUtils.CreatePlayableEntity(manager);
            var playable = PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            PlayableTestUtils.NewPlayableFromEntity(manager, entity, default(ClipSampler));
            
            manager.SetComponentData(entity, new PlayableTransitionRequest
            {
                PlayableId = (sbyte)playable.Id,
                TransitionDuration = 0.1f
            });
            
            var playables = manager.GetBuffer<PlayableState>(entity);
            Assert.AreEqual(3, playables.Length);
            //set all weights to random large numbers
            for (var i = 0; i < playables.Length; i++)
            {
                var p = playables[i];
                p.Weight = UnityEngine.Random.Range(2.1f, 5.1f);
                playables[i] = p;
            }
            
            UpdateWorld();
            
            playables = manager.GetBuffer<PlayableState>(entity);
            var sumWeights = playables.AsNativeArray().Sum(p => p.Weight);
            Assert.AreEqual(1, sumWeights);
        }
    }
}