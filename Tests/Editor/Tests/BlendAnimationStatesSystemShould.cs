using System.Linq;
using NUnit.Framework;
using Unity.Collections;
using AssertionException = UnityEngine.Assertions.AssertionException;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(BlendAnimationStatesSystem))]
    public class BlendAnimationStatesSystemShould : ECSTestsFixture
    {
        [Test]
        public void Create_New_AnimationState()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            Assert.GreaterOrEqual(animationState.Id, 0);
        }

        [Test]
        public void Reject_AnimationState_With_NoClips()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            Assert.Throws<AssertionException>(() =>
            {
                AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity,
                    new NativeArray<ClipSampler>(0, Allocator.Temp));
            });
        }

        [Test]
        public void Update_AnimationStates()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            Assert.Zero(animationState.Time);
            Assert.NotZero(animationState.Speed);
            
            //Force animationState to not be cleaned-up
            animationState.Weight = 1;
            AnimationStateTestUtils.SetAnimationState(manager, entity, animationState);
            UpdateWorld();
            animationState = AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, animationState.Id);
            Assert.Greater(animationState.Time, 0);
        }
        
        [Test]
        public void InstantlyTransition_If_CurrentState_IsNotValid()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            var currentState = manager.GetComponentData<AnimationCurrentState>(entity);
            Assert.IsFalse(currentState.IsValid);
            
            AnimationStateTestUtils.RequestTransitionTo(manager, entity, animationState.Id);
            
            //Force animationState to not be cleaned-up
            animationState.Weight = 1;
            AnimationStateTestUtils.SetAnimationState(manager, entity, animationState);
            UpdateWorld();
            animationState = AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, animationState.Id);
            Assert.Greater(animationState.Time, 0);
        }
        

        [Test]
        public void StartTransition_From_Request()
        {
            const float transitionDuration = 0.1f;
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, entity);
            Assert.IsFalse(manager.GetComponentData<AnimationCurrentState>(entity).IsValid);

            manager.SetComponentData(entity, new AnimationStateTransitionRequest
            {
                AnimationStateId = (sbyte)animationState.Id,
                TransitionDuration = transitionDuration
            });

            UpdateWorld();
            AnimationStateTestUtils.AssertOnGoingTransition(manager, entity, animationState.Id);
        }

        [Test]
        public void StartTransition_From_Request_Even_When_Transitioning()
        {
            const float transitionDuration = 0.1f;
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, entity);
            Assert.IsFalse(manager.GetComponentData<AnimationCurrentState>(entity).IsValid);

            manager.SetComponentData(entity, new AnimationStateTransitionRequest
            {
                AnimationStateId = (sbyte)animationState.Id,
                TransitionDuration = transitionDuration
            });

            UpdateWorld();
            AnimationStateTestUtils.AssertOnGoingTransition(manager, entity, animationState.Id);

            var secondAnimationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));

            manager.SetComponentData(entity, new AnimationStateTransitionRequest
            {
                AnimationStateId = (sbyte)secondAnimationState.Id
            });

            UpdateWorld();
            AnimationStateTestUtils.AssertOnGoingTransition(manager, entity, secondAnimationState.Id);
        }

        [Test]
        public void EndTransition_After_TransitionDuration()
        {
            const float transitionDuration = 0.1f;
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.SetCurrentState(manager, entity, 0);
            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, entity);
            
            AnimationStateTestUtils.AssertCurrentState(manager, entity, 0);
            AnimationStateTestUtils.RequestTransitionTo(manager, entity, animationState.Id, transitionDuration);

            //update a couple of times and make sure we're still transitioning
            for (var i = 0; i < 3; i++)
            {
                UpdateWorld();
                AnimationStateTestUtils.AssertOnGoingTransition(manager, entity, animationState.Id);
            }

            // make sure we complete the transition
            UpdateWorld(transitionDuration);
            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, entity);
            AnimationStateTestUtils.AssertCurrentState(manager, entity, animationState.Id);
        }

        [Test]
        public void IncreaseWeight_During_Transition()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var p1 = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            var p2 = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));

            AnimationStateTestUtils.AssertNoOnGoingTransition(manager, entity);
            AnimationStateTestUtils.SetCurrentState(manager, entity, p1.Id);
            AnimationStateTestUtils.RequestTransitionTo(manager, entity, p2.Id);

            var weight = p2.Weight;
            Assert.Zero(weight);
            for (var i = 0; i < 3; i++)
            {
                UpdateWorld();
                AnimationStateTestUtils.AssertOnGoingTransition(manager, entity, p2.Id);
                p2 = AnimationStateTestUtils.GetAnimationStateFromEntity(manager, entity, p2.Id);
                Assert.Greater(p2.Weight, weight);
                weight = p2.Weight;
            }
        }

        [Test]
        public void Cleanup_AnimationState_With_Zero_Weight()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            var clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.AreEqual(3, animationStates.Length);
            Assert.AreEqual(3, clipSamplers.Length);

            UpdateWorld();

            animationStates = manager.GetBuffer<AnimationState>(entity);
            clipSamplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.Zero(animationStates.Length);
            Assert.Zero(clipSamplers.Length);
        }

        [Test]
        public void NotCleanup_AnimationState_When_TransitionActive()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            Assert.AreEqual(3, animationStates.Length);

            //We need to assure weights of other clips are not zero, AnimationStatesSystem asserts this, since 
            for (var i = 1; i < animationStates.Length; i++)
            {
                var p = animationStates[i];
                p.Weight = 0.5f;
                animationStates[i] = p;
            }
            
            manager.SetComponentData(entity, new AnimationStateTransitionRequest
            {
                AnimationStateId = (sbyte)animationState.Id,
                TransitionDuration = 0.1f
            });
            
            UpdateWorld();

            animationStates = manager.GetBuffer<AnimationState>(entity);
            Assert.AreEqual(1, animationStates.Length);
            Assert.AreEqual(animationState.Id, animationStates[0].Id);
        }

        [Test]
        public void Keep_SumWeights_Equal_One()
        {
            var entity = AnimationStateTestUtils.CreateAnimationStateEntity(manager);
            var animationState = AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            AnimationStateTestUtils.NewAnimationStateFromEntity(manager, entity, default(ClipSampler));
            
            manager.SetComponentData(entity, new AnimationStateTransitionRequest
            {
                AnimationStateId = (sbyte)animationState.Id,
                TransitionDuration = 0.1f
            });
            
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            Assert.AreEqual(3, animationStates.Length);
            //set all weights to random large numbers
            for (var i = 0; i < animationStates.Length; i++)
            {
                var p = animationStates[i];
                p.Weight = UnityEngine.Random.Range(2.1f, 5.1f);
                animationStates[i] = p;
            }
            
            UpdateWorld();
            
            animationStates = manager.GetBuffer<AnimationState>(entity);
            var sumWeights = animationStates.AsNativeArray().Sum(p => p.Weight);
            Assert.AreEqual(1, sumWeights);
        }
    }
}