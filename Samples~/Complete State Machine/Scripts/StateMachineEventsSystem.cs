using BovineLabs.Event.Systems;
using DOTSAnimation;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public partial class StateMachineEventsSystem : SystemBase
{
    private struct AttackWindowEventJob : IJobAnimationEvent
    { 
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<AttackWindow> CfeAttackWindow;
        public void Execute(RaisedAnimationEvent e)
        {
            if (e.EventHash == AnimationEvents.StartAttackEventHash)
            {
                var atkWindow = CfeAttackWindow[e.AnimatorOwner];
                atkWindow.IsOpen = true;
                CfeAttackWindow[e.AnimatorOwner] = atkWindow;
            }
            else if (e.EventHash == AnimationEvents.EndAttackEventHash)
            {
                var atkWindow = CfeAttackWindow[e.AnimatorOwner];
                atkWindow.IsOpen = false;
                CfeAttackWindow[e.AnimatorOwner] = atkWindow;
            }
        }
    }
    
    private struct FootstepEventJob1 : IJobAnimationSingleEvent
    { 
        public int EventHash => AnimationEvents.FootstepEvent1;
        public void Execute(RaisedAnimationEvent e)
        {
            Debug.Log("Footstep 1");
        }
    }
    private struct FootstepEventJob2 : IJobAnimationSingleEvent
    { 
        public int EventHash => AnimationEvents.FootstepEvent2;
        public void Execute(RaisedAnimationEvent e)
        {
            Debug.Log("Footstep 2");
        }
    }

    private EventSystem animationEventsSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        animationEventsSystem = World.GetExistingSystem<EventSystem>();
    }

    protected override void OnUpdate()
    {
        //Example of job that manually read multiple event types
        Dependency = new AttackWindowEventJob
        {
            CfeAttackWindow = GetComponentDataFromEntity<AttackWindow>(),
        }.ScheduleParallel(animationEventsSystem, Dependency);
        
        //Example of IJobSingleAnimationEvent
        Dependency = new FootstepEventJob1()
        {
        }.ScheduleParallel(animationEventsSystem, Dependency);
        
        Dependency = new FootstepEventJob2()
        {
        }.ScheduleParallel(animationEventsSystem, Dependency);
    }
}