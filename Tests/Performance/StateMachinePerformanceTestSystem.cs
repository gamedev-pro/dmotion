using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;

namespace DMotion.PerformanceTests
{
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [BurstCompile]
    [RequireMatchingQueriesForUpdate]
    public partial struct StateMachinePerformanceTestSystem : ISystem
    {
        public static readonly ProfilerMarker Marker =
            new ProfilerMarker($"StateMachinePerformanceTestSystem: OnUpdate");

        public void OnCreate(ref SystemState state)
        {
        }

        public void OnDestroy(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dt = SystemAPI.Time.DeltaTime;
            var integerPart = (uint)SystemAPI.Time.ElapsedTime + 1;
            var decimalPart = (float)(SystemAPI.Time.ElapsedTime + 1) - integerPart;
            var shouldSwitchStates = decimalPart < dt && integerPart % 2 == 0;
            new UpdateStateMachineParametersJob
            {
                dt = dt,
                integerPart = integerPart,
                shouldSwitchStates = shouldSwitchStates,
                marker = Marker,
            }.ScheduleParallel();
        }

        [BurstCompile]
        partial struct UpdateStateMachineParametersJob : IJobEntity
        {
            public float dt;
            public uint integerPart;
            public bool shouldSwitchStates;
            public ProfilerMarker marker;

            public void Execute(Entity e,
                ref LinearBlendDirection linearBlendDirection,
                ref PlayOneShotRequest playOneShotRequest,
                ref DynamicBuffer<FloatParameter> blendParameters,
                ref DynamicBuffer<BoolParameter> boolParameters,
                in StressTestOneShotClip oneShotClip)
            {
                using var scope = marker.Auto();
                blendParameters[0] = new FloatParameter
                {
                    Hash = blendParameters[0].Hash,
                    Value = math.clamp(blendParameters[0].Value + linearBlendDirection.Value * dt, 0, 1)
                };

                if (shouldSwitchStates)
                {
                    var rnd = Random.CreateFromIndex((uint)(e.Index + integerPart));
                    var prob = rnd.NextInt(0, 101);
                    if (prob < 30)
                    {
                        linearBlendDirection.Value *= -1;
                    }
                    else if (prob < 60)
                    {
                        boolParameters[0] = new BoolParameter
                        {
                            Hash = boolParameters[0].Hash,
                            Value = !boolParameters[0].Value
                        };
                    }
                    else
                    {
                        playOneShotRequest =
                            new PlayOneShotRequest(oneShotClip.Clips, oneShotClip.ClipEvents, oneShotClip.ClipIndex);
                    }
                }
            }
        }
    }
}