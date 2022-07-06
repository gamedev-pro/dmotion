using System.Diagnostics.CodeAnalysis;
using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Unity.Jobs;

namespace DOTSAnimation
{
    public interface IJobAnimationEvent : IJobEvent<RaisedAnimationEvent>
    {
    }

    public static class JobAnimationEvent
    {
        public static JobHandle Schedule<TJob>(
            this TJob jobData,
            EventSystemBase eventSystem,
            JobHandle dependsOn = default)
            where TJob : struct, IJobAnimationEvent
        {
            return jobData.Schedule<TJob, RaisedAnimationEvent>(eventSystem, dependsOn);
        }

        public static JobHandle ScheduleParallel<TJob>(
            this TJob jobData,
            EventSystemBase eventSystem,
            JobHandle dependsOn = default)
            where TJob : struct, IJobAnimationEvent
        {
            return jobData.ScheduleParallel<TJob, RaisedAnimationEvent>(eventSystem, dependsOn);
        }
    }
}