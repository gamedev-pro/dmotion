using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace DMotion.Tests
{
    public static class PlayableTestUtils
    {
        public static void AssertNoTransitionRequest(EntityManager manager, Entity entity)
        {
            var playableTransitionRequest = manager.GetComponentData<PlayableTransitionRequest>(entity);
            Assert.IsFalse(playableTransitionRequest.IsValid,
                $"Expected invalid transition request, but requested is to {playableTransitionRequest.PlayableId}");
        }

        public static void AssertCurrentState(EntityManager manager, Entity entity, byte id)
        {
            var currentPlayableState = manager.GetComponentData<PlayableCurrentState>(entity);
            Assert.IsTrue(currentPlayableState.IsValid);
            Assert.AreEqual(id, currentPlayableState.PlayableId);
            var playable = GetPlayableFromEntity(manager, entity, id);
            Assert.AreEqual(1, playable.Weight);
        }

        public static void AssertNoOnGoingTransition(EntityManager manager, Entity entity)
        {
            AssertNoTransitionRequest(manager, entity);
            var playableTransition = manager.GetComponentData<PlayableTransition>(entity);
            Assert.IsFalse(playableTransition.IsValid,
                $"Expected invalid transition, but transitioning to {playableTransition.PlayableId}");
        }

        public static void AssertTransitionRequested(EntityManager manager, Entity entity, byte expectedPlayableId)
        {
            var playableTransitionRequest = manager.GetComponentData<PlayableTransitionRequest>(entity);
            Assert.IsTrue(playableTransitionRequest.IsValid);
        }

        public static void AssertOnGoingTransition(EntityManager manager, Entity entity, byte expectedPlayableId)
        {
            var playableTransitionRequest = manager.GetComponentData<PlayableTransitionRequest>(entity);
            Assert.IsFalse(playableTransitionRequest.IsValid);

            var playableTransition = manager.GetComponentData<PlayableTransition>(entity);
            Assert.IsTrue(playableTransition.IsValid, "Expect current transition to be active");
            Assert.AreEqual(expectedPlayableId, playableTransition.PlayableId, $"Current transition ({playableTransition.PlayableId}) different from expected it {expectedPlayableId}");
        }

        internal static Entity CreatePlayableEntity(EntityManager manager)
        {
            var newEntity = manager.CreateEntity(
                typeof(PlayableState),
                typeof(ClipSampler));

            manager.AddComponentData(newEntity, PlayableCurrentState.Null);
            manager.AddComponentData(newEntity, PlayableTransitionRequest.Null);
            manager.AddComponentData(newEntity, PlayableTransition.Null);
            return newEntity;
        }

        internal static void SetPlayable(EntityManager manager, Entity entity, PlayableState playable)
        {
            var playables = manager.GetBuffer<PlayableState>(entity);
            var index = playables.IdToIndex(playable.Id);
            Assert.GreaterOrEqual(index, 0);
            playables[index] = playable;
        }

        internal static void SetCurrentState(EntityManager manager, Entity entity, byte playableId)
        {
            manager.SetComponentData(entity, new PlayableCurrentState{PlayableId = (sbyte) playableId});
            var playable = GetPlayableFromEntity(manager, entity, playableId);
            playable.Weight = 1;
            SetPlayable(manager, entity, playable);
        }
        
        internal static void TransitionTo(EntityManager manager, Entity entity, byte playableId,
            float transitionDuration = 0.1f)
        {
            manager.SetComponentData(entity, new PlayableTransitionRequest
            {
                PlayableId = (sbyte)playableId,
                TransitionDuration = transitionDuration
            });
        }

        internal static PlayableState GetPlayableFromEntity(EntityManager manager, Entity entity, byte playableId)
        {
            var playables = manager.GetBuffer<PlayableState>(entity);
            return playables.GetWithId(playableId);
        }

        internal static PlayableState NewPlayableFromEntity(EntityManager manager, Entity entity,
            ClipSampler newSampler,
            float speed = 1, bool loop = true)
        {
            var playableStates = manager.GetBuffer<PlayableState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            var playableIndex = PlayableState.New(ref playableStates, ref samplers, newSampler, speed, loop);
            Assert.GreaterOrEqual(playableIndex, 0);
            Assert.IsTrue(playableStates.ExistsWithId(playableStates[playableIndex].Id));
            return playableStates[playableIndex];
        }

        internal static PlayableState NewPlayableFromEntity(EntityManager manager, Entity entity,
            NativeArray<ClipSampler> newSamplers,
            float speed = 1, bool loop = true)
        {
            var playableStates = manager.GetBuffer<PlayableState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            var playableIndex = PlayableState.New(ref playableStates, ref samplers, newSamplers, speed, loop);
            Assert.GreaterOrEqual(0, playableIndex);
            Assert.IsTrue(playableStates.ExistsWithId(playableStates[playableIndex].Id));
            return playableStates[playableIndex];
        }
    }
}