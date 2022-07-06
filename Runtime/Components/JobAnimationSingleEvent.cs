using System;
using BovineLabs.Event.Containers;
using BovineLabs.Event.Jobs;
using BovineLabs.Event.Systems;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace DOTSAnimation
{
    public interface IJobAnimationSingleEvent : IJobEvent<RaisedAnimationEvent>
    {
        public int EventHash { get; }
    }
    public static class JobAnimationSingleEvent
    {
        public static JobHandle Schedule<TJob>(
            this TJob jobData,
            EventSystemBase eventSystem,
            JobHandle dependsOn = default)
            where TJob : struct, IJobAnimationSingleEvent
        {
            return ScheduleInternal(jobData, eventSystem, dependsOn, jobData.EventHash, false);
        }

        public static JobHandle ScheduleParallel<TJob>(
            this TJob jobData,
            EventSystemBase eventSystem,
            JobHandle dependsOn = default)
            where TJob : struct, IJobAnimationSingleEvent
        {
            return ScheduleInternal(jobData, eventSystem, dependsOn, jobData.EventHash, true);
        }
        
        private static unsafe JobHandle ScheduleInternal<TJob>(
            this TJob jobData,
            EventSystemBase eventSystem,
            JobHandle dependsOn,
            int eventHash,
            bool isParallel)
            where TJob : struct, IJobAnimationSingleEvent
        {
            dependsOn = eventSystem.GetEventReaders<RaisedAnimationEvent>(dependsOn, out var events);

            for (var i = 0; i < events.Count; i++)
            {
                var reader = events[i];

                var fullData = new JobEventProducer<TJob>
                {
                    Reader = reader,
                    EventHash = eventHash,
                    JobData = jobData,
                    IsParallel = isParallel,
                };

#if UNITY_2020_2_OR_NEWER
                const ScheduleMode scheduleMode = ScheduleMode.Parallel;
#else
                const ScheduleMode scheduleMode = ScheduleMode.Batched;
#endif

                var scheduleParams = new JobsUtility.JobScheduleParameters(
                    UnsafeUtility.AddressOf(ref fullData),
                    isParallel ? JobEventProducer<TJob>.InitializeParallel() : JobEventProducer<TJob>.InitializeSingle(),
                    dependsOn,
                    scheduleMode);

                dependsOn = isParallel
                    ? JobsUtility.ScheduleParallelFor(ref scheduleParams, reader.ForEachCount, 1)
                    : JobsUtility.Schedule(ref scheduleParams);
            }

            eventSystem.AddJobHandleForConsumer<RaisedAnimationEvent>(dependsOn);

            return dependsOn;
        }

        internal struct JobEventProducer<TJob>
            where TJob : struct, IJobAnimationSingleEvent
        {
            [ReadOnly]
            public NativeEventStream.Reader Reader;

            public int EventHash;

            public TJob JobData;

            public bool IsParallel;

            private static IntPtr jobReflectionDataSingle;

            private static IntPtr jobReflectionDataParallel;

            private delegate void ExecuteJobFunction(
                ref JobEventProducer<TJob> fullData,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex);

            /// <summary> Initializes the job. </summary>
            /// <returns> The job pointer. </returns>
            public static IntPtr InitializeSingle()
            {
                if (jobReflectionDataSingle == IntPtr.Zero)
                {
#if UNITY_2020_2_OR_NEWER
                    jobReflectionDataSingle = JobsUtility.CreateJobReflectionData(
                        typeof(JobEventProducer<TJob>),
                        typeof(TJob),
                        (ExecuteJobFunction)Execute);
#else
                    jobReflectionDataSingle = JobsUtility.CreateJobReflectionData(
                        typeof(JobEventProducer<TJob>),
                        typeof(TJob),
                        JobType.Single,
                        (ExecuteJobFunction)Execute);
#endif
                }

                return jobReflectionDataSingle;
            }

            public static IntPtr InitializeParallel()
            {
                if (jobReflectionDataParallel == IntPtr.Zero)
                {
#if UNITY_2020_2_OR_NEWER
                    jobReflectionDataParallel = JobsUtility.CreateJobReflectionData(
                        typeof(JobEventProducer<TJob>),
                        typeof(TJob),
                        (ExecuteJobFunction)Execute);
#else
                    jobReflectionDataParallel = JobsUtility.CreateJobReflectionData(
                        typeof(JobEventProducer<TJob>),
                        typeof(TJob),
                        JobType.ParallelFor,
                        (ExecuteJobFunction)Execute);
#endif
                }

                return jobReflectionDataParallel;
            }

            public static void Execute(
                ref JobEventProducer<TJob> fullData,
                IntPtr additionalPtr,
                IntPtr bufferRangePatchData,
                ref JobRanges ranges,
                int jobIndex)
            {
                while (true)
                {
                    int begin = 0;
                    int end = fullData.Reader.ForEachCount;

                    // If we are running the job in parallel, steal some work.
                    if (fullData.IsParallel)
                    {
                        if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out begin, out end))
                        {
                            return;
                        }
                    }

                    for (int i = begin; i < end; i++)
                    {
                        var count = fullData.Reader.BeginForEachIndex(i);

                        for (var j = 0; j < count; j++)
                        {
                            var e = fullData.Reader.Read<RaisedAnimationEvent>();
                            if (e.EventHash == fullData.EventHash)
                            {
                                fullData.JobData.Execute(e);
                            }
                        }

                        fullData.Reader.EndForEachIndex();
                    }

                    if (!fullData.IsParallel)
                    {
                        break;
                    }
                }
            }
        }
    }
}