using System;
using System.Linq;
using DMotion.Authoring;
using NUnit.Framework;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(AnimationEventsSystem))]
    public class RaiseAnimationEventsJobShould : ECSTestsFixture
    {
        [SerializeField, ConvertGameObjectPrefab(nameof(stateMachineEntityPrefab))]
        private AnimationStateMachineAuthoring stateMachinePrefab;
        
        [SerializeField]
        private AnimationClipAsset clipWithEvents;

        private Entity stateMachineEntityPrefab;

        private int clipWithEventsIndex = -1;

        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Assert.IsNotNull(clipWithEvents);
            Assert.NotZero(clipWithEvents.Events.Length);
            var stateMachineAsset = stateMachinePrefab.GetComponent<AnimationStateMachineAuthoring>().StateMachineAsset;
            clipWithEventsIndex = stateMachineAsset.Clips.ToList().IndexOf(clipWithEvents);
            Assert.IsTrue(clipWithEventsIndex >= 0);
        }

        [Test]
        public void ResetRaisedEventsEveryFrame()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            var raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            raisedEvents.Add(default);
            Assert.NotZero(raisedEvents.Length);
            UpdateWorld();
            
            raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.Zero(raisedEvents.Length);
        }
        
        [Test]
        public void Raise_When_EventTime_BetweenPreviousAndCurrentTime()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();

            var raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.Zero(raisedEvents.Length);
            
            GetEventAndClipSampler(out var clipSampler, out var eventToRaise);
            clipSampler.PreviousTime = eventToRaise.ClipTime - 0.1f;
            clipSampler.Time = eventToRaise.ClipTime + 0.1f;
            manager.GetBuffer<ClipSampler>(newEntity).Add(clipSampler);
            
            UpdateWorld();
            raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.AreEqual(1, raisedEvents.Length);
            var raised = raisedEvents[0];
            Assert.AreEqual(raised.EventHash, eventToRaise.EventHash);
        }


        [Test]
        public void Raise_When_EventTime_Equals_CurrentTime()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();

            var raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.Zero(raisedEvents.Length);
            
            GetEventAndClipSampler(out var clipSampler, out var eventToRaise);
            clipSampler.PreviousTime = eventToRaise.ClipTime - 0.1f;
            clipSampler.Time = eventToRaise.ClipTime;
            manager.GetBuffer<ClipSampler>(newEntity).Add(clipSampler);
            
            UpdateWorld();
            raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.AreEqual(1, raisedEvents.Length);
            var raised = raisedEvents[0];
            Assert.AreEqual(raised.EventHash, eventToRaise.EventHash);
        }
        
        [Test(Description = "Events should not be raised when EventTime == Previous Time since they will have already be raised in the previous frameb, when Event Time == Current Time")]
        
        public void NotRaise_When_EventTime_Equals_PreviousTime()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();

            var raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.Zero(raisedEvents.Length);
            
            GetEventAndClipSampler(out var clipSampler, out var eventToRaise);
            clipSampler.PreviousTime = eventToRaise.ClipTime;
            clipSampler.Time = eventToRaise.ClipTime + 0.1f;
            manager.GetBuffer<ClipSampler>(newEntity).Add(clipSampler);
            
            UpdateWorld();
            raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.Zero(raisedEvents.Length);
        }
        
        [Test(Description = "For when the clip loops. Ex: EventTime = 0.1f, Previous Time = 0.9f, Time = 0.2f")]
        public void Raise_When_EventTime_BetweenCurrentAndPreviousTime()
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            UpdateWorld();

            var raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.Zero(raisedEvents.Length);

            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            ref var clip = ref stateMachine.ClipsBlob.Value.clips[clipWithEventsIndex];
            GetEventAndClipSampler(out var clipSampler, out var eventToRaise);
            clipSampler.PreviousTime = clip.duration - math.EPSILON;
            clipSampler.Time = eventToRaise.ClipTime + math.EPSILON;
            Assert.Greater(clipSampler.PreviousTime, clipSampler.Time, "This test expect Previous time is greater than Clip Time (clip loops). Something is wrong with the test setup, or the event time");
            Assert.Less(clipSampler.Time, clip.duration, "Event is too near the end of the clip. Please test an event that is close to the beginning of the clip");
            manager.GetBuffer<ClipSampler>(newEntity).Add(clipSampler);
            
            UpdateWorld();
            raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.AreEqual(1, raisedEvents.Length);
            var raised = raisedEvents[0];
            Assert.AreEqual(raised.EventHash, eventToRaise.EventHash);
        }

        private void GetEventAndClipSampler(out ClipSampler clipSampler, out AnimationClipEvent clipEvent)
        {
            var newEntity = manager.InstantiateStateMachineEntity(stateMachineEntityPrefab);
            var stateMachine = manager.GetComponentData<AnimationStateMachine>(newEntity);
            ref var events = ref stateMachine.ClipEventsBlob.Value.ClipEvents[clipWithEventsIndex];
            Assert.NotZero(events.Events.Length);

            clipEvent = events.Events[0];
            clipSampler = new ClipSampler
            {
                Clips = stateMachine.ClipsBlob,
                ClipEventsBlob = stateMachine.ClipEventsBlob,
                ClipIndex = (ushort)clipWithEventsIndex,
                PreviousTime = 0,
                Time = 0,
                Weight = 1
            };
        }
    }
}