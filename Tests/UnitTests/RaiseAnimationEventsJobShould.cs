using DMotion.Authoring;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace DMotion.Tests
{
    [CreateSystemsForTest(typeof(AnimationEventsSystem))]
    public class RaiseAnimationEventsJobShould : ECSTestBase
    {
        private float[] eventTimes = new[] { 0.2f, 0.5f };

        [Test]
        public void Run_With_Valid_Queries()
        {
            CreateEntityWithClipPlaying(out _, out _, out _);
            UpdateWorld();
            ECSTestUtils.AssertSystemQueries<AnimationEventsSystem>(world);
        }

        [Test]
        public void ResetRaisedEventsEveryFrame()
        {
            CreateEntityWithClipPlaying(out var newEntity, out _, out _);
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
            CreateEntityWithClipPlaying(out var newEntity, out var samplerIndex, out var animationClipEvents);

            var eventToRaise = animationClipEvents[0];
            UpdateWorld(eventToRaise.ClipTime * 0.5f);

            AssertNoRaisedEvents(newEntity);

            SetTimeAndPreviousTimeForSampler(newEntity, samplerIndex, eventToRaise.ClipTime - 0.1f,
                eventToRaise.ClipTime + 0.1f);

            UpdateWorld();
            AssertEventRaised(newEntity, eventToRaise.EventHash);
        }


        [Test]
        public void Raise_When_EventTime_Equals_CurrentTime()
        {
            CreateEntityWithClipPlaying(out var newEntity, out var samplerIndex, out var events);

            var eventToRaise = events[0];
            UpdateWorld(eventToRaise.ClipTime * 0.5f);

            AssertNoRaisedEvents(newEntity);

            var prevTime = eventToRaise.ClipTime - 0.1f;
            var time = eventToRaise.ClipTime;
            SetTimeAndPreviousTimeForSampler(newEntity, samplerIndex, prevTime, time);

            UpdateWorld();
            AssertEventRaised(newEntity, eventToRaise.EventHash);
        }

        [Test(Description =
            "Events should not be raised when EventTime == Previous Time since they will have already be raised in the previous frameb, when Event Time == Current Time")]
        public void NotRaise_When_EventTime_Equals_PreviousTime()
        {
            CreateEntityWithClipPlaying(out var newEntity, out var samplerIndex, out var events);
            UpdateWorld();

            AssertNoRaisedEvents(newEntity);

            var eventToRaise = events[0];

            var prevTime = eventToRaise.ClipTime;
            var time = eventToRaise.ClipTime + 0.1f;
            SetTimeAndPreviousTimeForSampler(newEntity, samplerIndex, prevTime, time);

            UpdateWorld();
            AssertNoRaisedEvents(newEntity);
        }

        [Test(Description = "For when the clip loops. Ex: EventTime = 0.1f, Previous Time = 0.9f, Time = 0.2f")]
        public void Raise_When_EventTime_BetweenCurrentAndPreviousTime()
        {
            CreateEntityWithClipPlaying(out var newEntity, out var samplerIndex, out var events);
            UpdateWorld();

            AssertNoRaisedEvents(newEntity);

            var eventToRaise = events[0];
            var sampler = manager.GetBuffer<ClipSampler>(newEntity);
            ref var clip = ref sampler[samplerIndex].Clip;

            var prevTime = clip.duration - math.EPSILON;
            var time = eventToRaise.ClipTime + math.EPSILON;
            Assert.Greater(prevTime, time,
                "This test expect Previous time is greater than Clip Time (clip loops). Something is wrong with the test setup, or the event time");
            Assert.Less(time, clip.duration,
                "Event is too near the end of the clip. Please test an event that is close to the beginning of the clip");
            SetTimeAndPreviousTimeForSampler(newEntity, samplerIndex, prevTime, time);

            UpdateWorld();
            AssertEventRaised(newEntity, eventToRaise.EventHash);
        }

        private void CreateEntityWithClipPlaying(out Entity entity, out int samplerIndex,
            out AnimationClipEvent[] events)
        {
            entity = manager.CreateEntity();
            AnimationStateMachineConversionUtils.AddSingleClipStateComponents(manager, entity, entity,
                true, false, RootMotionMode.Disabled);

            //create fake animation clip asset
            var animationClipAsset = ScriptableObject.CreateInstance<AnimationClipAsset>();
            {
                var animationEventName1 = ScriptableObject.CreateInstance<AnimationEventName>();
                var animationEventName2 = ScriptableObject.CreateInstance<AnimationEventName>();
                animationEventName1.name = "Event1";
                animationEventName2.name = "Event2";
                animationClipAsset.Events = new[]
                {
                    new Authoring.AnimationClipEvent { Name = animationEventName1, NormalizedTime = eventTimes[0] },
                    new Authoring.AnimationClipEvent { Name = animationEventName2, NormalizedTime = eventTimes[1] }
                };

                var clip = new AnimationClip();
                var curve = AnimationCurve.Linear(0, 0, 1, 1);
                clip.SetCurve("", typeof(Transform), "localPosition.x", curve);

                clip.name = "Move";
                clip.legacy = true;

                animationClipAsset.Clip = clip;
            }

            var clipEventsBlob =
                ClipEventsAuthoringUtils.CreateClipEventsBlob(new[] { animationClipAsset });

            Assert.AreEqual(1, clipEventsBlob.Value.ClipEvents.Length);
            Assert.AreEqual(2, clipEventsBlob.Value.ClipEvents[0].Events.Length);
            events = clipEventsBlob.Value.ClipEvents[0].Events.ToArray();

            var singleState = AnimationStateTestUtils.CreateSingleClipState(manager, entity, clipEventsBlob);
            AnimationStateTestUtils.SetCurrentState(manager, entity, singleState.AnimationStateId);

            var samplers = manager.GetBuffer<ClipSampler>(entity);
            Assert.AreEqual(1, samplers.Length);
            samplerIndex = 0;
            var sampler = samplers[samplerIndex];
            sampler.Weight = 1;
            samplers[samplerIndex] = sampler;
        }

        private void AssertNoRaisedEvents(Entity newEntity)
        {
            var raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.Zero(raisedEvents.Length);
        }

        private void AssertEventRaised(Entity newEntity, int expectedHash)
        {
            var raisedEvents = manager.GetBuffer<RaisedAnimationEvent>(newEntity);
            Assert.AreEqual(1, raisedEvents.Length);
            var raised = raisedEvents[0];
            Assert.AreEqual(raised.EventHash, expectedHash);
        }

        private void SetTimeAndPreviousTimeForSampler(Entity newEntity, int samplerIndex, float prevTime, float time)
        {
            var clipSamplers = manager.GetBuffer<ClipSampler>(newEntity);
            var clipSampler = clipSamplers[samplerIndex];
            clipSampler.PreviousTime = prevTime;
            clipSampler.Time = time;

            clipSamplers[samplerIndex] = clipSampler;
        }
    }
}