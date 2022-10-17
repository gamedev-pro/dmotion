using DMotion;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace DMotion.Samples.CompleteStateMachine
{
    public enum MovementMode
    {
        Normal,
        Crouching,
        Strafing,
    }
    
    public struct StateMachineExampleEvents : IComponentData
    {
        public int StartAttackEventHash;
        public int EndAttackEventHash;
        public int FootStepEventHash;
    }

    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(AnimationEventsSystem))]
    public partial class StateMachineEventsSystem : SystemBase
    {
        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<StateMachineExampleEvents>();
        }

        protected override void OnUpdate()
        {
            var exampleEvents = GetSingleton<StateMachineExampleEvents>();

            var cfeAtckWindow = GetComponentDataFromEntity<AttackWindow>();
            Entities
                .WithNativeDisableContainerSafetyRestriction(cfeAtckWindow)
                .ForEach((in AnimatorEntity animatorEntity, in DynamicBuffer<RaisedAnimationEvent> raisedEvents) =>
                {
                    for (var i = 0; i < raisedEvents.Length; i++)
                    {
                        if (raisedEvents[i].EventHash == exampleEvents.StartAttackEventHash)
                        {
                            var atkWindow = cfeAtckWindow[animatorEntity.Owner];
                            atkWindow.IsOpen = true;
                            cfeAtckWindow[animatorEntity.Owner] = atkWindow;
                            Debug.Log("Opening attack window");
                        }
                        if (raisedEvents[i].EventHash == exampleEvents.EndAttackEventHash)
                        {
                            var atkWindow = cfeAtckWindow[animatorEntity.Owner];
                            atkWindow.IsOpen = false;
                            cfeAtckWindow[animatorEntity.Owner] = atkWindow;
                            Debug.Log("Closing attack window");
                        }
                    }
                }).ScheduleParallel();


            Entities.ForEach((in DynamicBuffer<RaisedAnimationEvent> raisedEvents) =>
            {
                for (var i = 0; i < raisedEvents.Length; i++)
                {
                    if (raisedEvents[i].EventHash == exampleEvents.FootStepEventHash)
                    {
                        var str = FixedString.Format("Footstep raised by clip: {0} (w: {1})",
                            raisedEvents[i].ClipHandle.Clip.name, raisedEvents[i].ClipWeight);
                        Debug.Log(str);
                    }
                }
            }).ScheduleParallel();
        }
    }
}