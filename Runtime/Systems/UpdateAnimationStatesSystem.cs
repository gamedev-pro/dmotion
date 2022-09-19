using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace DMotion
{
    [UpdateInGroup(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(BlendAnimationStatesSystem))]
    internal partial class UpdateAnimationStatesSystem : SystemBase
    {
        internal EntityQuery updateSingleClipsQuery;
        internal EntityQuery cleanSingleClipsQuery;
        internal EntityQuery updateLinearBlendClipsQuery;
        internal EntityQuery cleanLinearBlendClipsQuery;
        
        protected override void OnCreate()
        {
            base.OnCreate();
            updateSingleClipsQuery = GetEntityQuery(new EntityQueryDesc{All = new[]{ComponentType.ReadOnly<SingleClipState>(), ComponentType.ReadOnly<AnimationState>(), ComponentType.ReadWrite<ClipSampler>()}, Any = new ComponentType[]{}, None = new ComponentType[]{}, Options = EntityQueryOptions.Default});
            cleanSingleClipsQuery = GetEntityQuery(new EntityQueryDesc{All = new []{ComponentType.ReadOnly<AnimationState>(), ComponentType.ReadWrite<SingleClipState>()}, Any = new ComponentType[]{}, None = new ComponentType[]{}, Options = EntityQueryOptions.Default});
            updateLinearBlendClipsQuery = GetEntityQuery(new EntityQueryDesc{All = new[]{ComponentType.ReadOnly<AnimationState>(), ComponentType.ReadOnly<LinearBlendStateMachineState>(), ComponentType.ReadOnly<BlendParameter>(), ComponentType.ReadWrite<ClipSampler>()}, Any = new ComponentType[]{}, None = new ComponentType[]{}, Options = EntityQueryOptions.Default});
            cleanLinearBlendClipsQuery = GetEntityQuery(new EntityQueryDesc{All = new []{ComponentType.ReadOnly<AnimationState>(), ComponentType.ReadWrite<LinearBlendStateMachineState>()}, Any = new ComponentType[]{}, None = new ComponentType[]{}, Options = EntityQueryOptions.Default});
        }

        protected override void OnUpdate()
        {
            var singleClipHandle = new UpdateSingleClipStatesJob
            {
                DeltaTime = Time.DeltaTime,
                ClipSamplersHandle = GetBufferTypeHandle<ClipSampler>(false),
                AnimationStatesHandle = GetBufferTypeHandle<AnimationState>(true),
                SingleClipStatesHandle = GetBufferTypeHandle<SingleClipState>(true)
            }.ScheduleParallel(updateSingleClipsQuery, Dependency);

            singleClipHandle = new CleanSingleClipStatesJob
            {
                SingleClipStatesHandle = GetBufferTypeHandle<SingleClipState>(false),
                AnimationStatesHandle = GetBufferTypeHandle<AnimationState>(true)
            }.ScheduleParallel(cleanSingleClipsQuery, singleClipHandle);
            
            var linearBlendHandle = new UpdateLinearBlendStateMachineStatesJob
            {
                DeltaTime = Time.DeltaTime,
                ClipSamplersHandle = GetBufferTypeHandle<ClipSampler>(false),
                AnimationStatesHandle = GetBufferTypeHandle<AnimationState>(true),
                LinearBlendStateMachineStatesHandle = GetBufferTypeHandle<LinearBlendStateMachineState>(true),
                BlendParametersStateHandle = GetBufferTypeHandle<BlendParameter>(true)
            }.ScheduleParallel(updateLinearBlendClipsQuery, Dependency);

            linearBlendHandle = new CleanLinearBlendStatesJob
            {
                LinearBlendStateMachineStates = GetBufferTypeHandle<LinearBlendStateMachineState>(false),
                AnimationStatesHandle = GetBufferTypeHandle<AnimationState>(true)
            }.ScheduleParallel(cleanLinearBlendClipsQuery, linearBlendHandle);
            
            Dependency = JobHandle.CombineDependencies(singleClipHandle, linearBlendHandle);
        }
    }
}