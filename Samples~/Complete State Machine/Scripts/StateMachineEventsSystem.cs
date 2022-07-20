using BovineLabs.Event.Systems;
using DOTSAnimation;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public struct StateMachineExampleEvents : IComponentData
{
    public int StartAttackEventHash;
    public int EndAttackEventHash;
    public int FootStepEventHash;
}

public partial class StateMachineEventsSystem : SystemBase
{
    private struct AttackWindowEventJob : IJobAnimationEvent
    {
        public int StartAttackEventHash;
        public int EndAttackEventHash;
        
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<AttackWindow> CfeAttackWindow;
        public void Execute(RaisedAnimationEvent e)
        {
            if (e.EventHash == StartAttackEventHash)
            {
                var atkWindow = CfeAttackWindow[e.AnimatorOwner];
                atkWindow.IsOpen = true;
                CfeAttackWindow[e.AnimatorOwner] = atkWindow;
            }
            else if (e.EventHash == EndAttackEventHash)
            {
                var atkWindow = CfeAttackWindow[e.AnimatorOwner];
                atkWindow.IsOpen = false;
                CfeAttackWindow[e.AnimatorOwner] = atkWindow;
            }
        }
    }
    
    private struct FootstepEventJob : IJobAnimationSingleEvent
    {
        public int FootStepEventHash;
        public int EventHash => FootStepEventHash;
        public void Execute(RaisedAnimationEvent e)
        {
            Debug.Log("Footstep");
        }
    }

    private EventSystem animationEventsSystem;

    protected override void OnCreate()
    {
        base.OnCreate();
        animationEventsSystem = World.GetExistingSystem<EventSystem>();
        RequireSingletonForUpdate<StateMachineExampleEvents>();
    }

    protected override void OnUpdate()
    {
        var exampleEvents = GetSingleton<StateMachineExampleEvents>();
        //Example of job that manually read multiple event types
        Dependency = new AttackWindowEventJob
        {
            StartAttackEventHash = exampleEvents.StartAttackEventHash,
            EndAttackEventHash = exampleEvents.EndAttackEventHash,
            CfeAttackWindow = GetComponentDataFromEntity<AttackWindow>(),
        }.ScheduleParallel(animationEventsSystem, Dependency);
        
        //Example of IJobSingleAnimationEvent
        Dependency = new FootstepEventJob()
        {
            FootStepEventHash = exampleEvents.FootStepEventHash
        }.ScheduleParallel(animationEventsSystem, Dependency);
    }
}