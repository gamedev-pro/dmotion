using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Profiling;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateBefore(typeof(AnimationStateMachineSystem))]
    public partial class PlayOneShotSystem : SystemBase
    {
        [BurstCompile]
        internal partial struct PlayOneShotJob : IJobEntity
        {
            internal float DeltaTime;
            internal ProfilerMarker Marker;

            internal void Execute(
                ref AnimationStateMachine stateMachine,
                ref DynamicBuffer<ClipSampler> clipSamplers,
                ref PlayOneShotRequest playOneShot,
                ref OneShotState oneShotState
            )
            {
                using var scope = Marker.Auto();
                //Evaluate requested one shot
                {
                    if (playOneShot.IsValid)
                    {
                        var clipSampler = new ClipSampler
                        {
                            ClipIndex = (byte)playOneShot.ClipIndex,
                            Clips = playOneShot.Clips,
                            ClipEventsBlob = playOneShot.ClipEvents,
                            Time = 0,
                            PreviousTime = 0,
                            Weight = 1
                        };
                        var newSamplerId = clipSamplers.AddWithId(clipSampler);

                        oneShotState = new OneShotState(newSamplerId,
                            playOneShot.TransitionDuration,
                            playOneShot.EndTime * clipSampler.Clip.duration,
                            playOneShot.Speed);

                        playOneShot = PlayOneShotRequest.Null;
                    }
                }

                //Update One shot
                {
                    if (oneShotState.IsValid)
                    {
                        var samplerIndex = clipSamplers.IdToIndex((byte)oneShotState.SamplerId);
                        var sampler = clipSamplers[samplerIndex];
                        sampler.PreviousTime = sampler.Time;
                        sampler.Time += DeltaTime * oneShotState.Speed;

                        float oneShotWeight;
                        //blend out
                        if (sampler.Time > oneShotState.EndTime)
                        {
                            var blendOutTime = sampler.Clip.duration - oneShotState.EndTime;
                            if (!mathex.iszero(blendOutTime))
                            {
                                oneShotWeight = math.clamp((sampler.Clip.duration - sampler.Time) /
                                                           blendOutTime, 0, 1);
                            }
                            else
                            {
                                oneShotWeight = 0;
                            }
                        }
                        //blend in
                        else
                        {
                            oneShotWeight = math.clamp(sampler.Time /
                                                       oneShotState.TransitionDuration, 0, 1);
                        }

                        sampler.Weight = oneShotWeight;
                        // stateMachine.Weight = 1 - oneShotWeight;

                        clipSamplers[samplerIndex] = sampler;

                        //if blend out finished
                        if (sampler.Time >= sampler.Clip.duration)
                        {
                            // stateMachine.Weight = 1;
                            clipSamplers.RemoveAt(samplerIndex);
                            oneShotState = OneShotState.Null;
                        }
                    }
                }
            }
        }
        
        internal static readonly ProfilerMarker Marker_PlayOneShot =
            ProfilingUtils.CreateAnimationMarker<PlayOneShotSystem>(nameof(PlayOneShotJob));

        protected override void OnUpdate()
        {
            // new PlayOneShotJob
            // {
            //     DeltaTime = Time.DeltaTime,
            //     Marker = Marker_PlayOneShot
            // }.ScheduleParallel();
        }
    }
}