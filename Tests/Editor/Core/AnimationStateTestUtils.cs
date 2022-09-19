using Latios.Kinemation;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace DMotion.Tests
{
    internal static class AnimationStateTestUtils
    {
        internal static void SetBlendParameter(in LinearBlendStateMachineState linearBlendState, EntityManager manager,
            Entity entity, float value)
        {
            var blendParams = manager.GetBuffer<BlendParameter>(entity);
            ref var blob = ref linearBlendState.LinearBlendBlob;
            var blendRatio = blendParams[blob.BlendParameterIndex];
            blendRatio.Value = value;
            blendParams[blob.BlendParameterIndex] = blendRatio;
        }
        
        internal static void FindActiveSamplerIndexesForLinearBlend(
            in LinearBlendStateMachineState linearBlendState,
            EntityManager manager, Entity entity,
            out int firstClipIndex, out int secondClipIndex)
        {
            var blendParams = manager.GetBuffer<BlendParameter>(entity);
            LinearBlendStateUtils.ExtractLinearBlendVariablesFromStateMachine(
                linearBlendState, blendParams,
                out var blendRatio, out var thresholds);
            LinearBlendStateUtils.FindActiveClipIndexes(blendRatio, thresholds, out firstClipIndex, out secondClipIndex);
            var startIndex = ClipSamplerTestUtils.PlayableStartSamplerIdToIndex(manager, entity, linearBlendState.PlayableId);
            firstClipIndex += startIndex;
            secondClipIndex += startIndex;
        }

        internal static LinearBlendStateMachineState CreateLinearBlendForStateMachine(short stateIndex, EntityManager manager, Entity entity)
        {
            Assert.GreaterOrEqual(stateIndex, 0);
            var stateMachine = manager.GetComponentData<AnimationStateMachine>(entity);
            Assert.IsTrue(stateIndex < stateMachine.StateMachineBlob.Value.States.Length);
            Assert.AreEqual(StateType.LinearBlend, stateMachine.StateMachineBlob.Value.States[stateIndex].Type);
            var linearBlend = manager.GetBuffer<LinearBlendStateMachineState>(entity);
            var playables = manager.GetBuffer<PlayableState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            return LinearBlendStateUtils.NewForStateMachine(stateIndex,
                stateMachine.StateMachineBlob,
                stateMachine.ClipsBlob,
                stateMachine.ClipEventsBlob,
                ref linearBlend,
                ref playables,
                ref samplers
            );
        }
        
        internal static SingleClipState CreateSingleClipState(EntityManager manager, Entity entity,
            float speed = 1.0f,
            bool loop = false,
            ushort clipIndex = 0)
        {
            var singleClips = manager.GetBuffer<SingleClipState>(entity);
            var playableStates = manager.GetBuffer<PlayableState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);

            var clipsBlob = CreateFakeSkeletonClipSetBlob(1);

            return SingleClipStateUtils.New(
                clipIndex, speed, loop,
                clipsBlob,
                BlobAssetReference<ClipEventsBlob>.Null,
                ref singleClips,
                ref playableStates,
                ref samplers
            );
        }

        internal static BlobAssetReference<SkeletonClipSetBlob> CreateFakeSkeletonClipSetBlob(int clipCount)
        {
            Assert.Greater(clipCount, 0);
            var     builder = new BlobBuilder(Allocator.Temp);
            ref var root    = ref builder.ConstructRoot<SkeletonClipSetBlob>();
            root.boneCount = 1;
            var blobClips   = builder.Allocate(ref root.clips, clipCount);
            for (int i = 0; i < clipCount; i++)
            {
                blobClips[i] = new SkeletonClip()
                {
                    duration = 1,
                    sampleRate = 1,
                    boneCount = 1,
                    name = $"Dummy Clip {i}"
                };
            }
            
            return builder.CreateBlobAssetReference<SkeletonClipSetBlob>(Allocator.Temp);
        }
    }
}