using Latios.Kinemation;
using NUnit.Framework;
using Unity.Collections;
using Unity.Entities;

namespace DMotion.Tests
{
    public static class AnimationStateTestUtils
    {
        public static void AssertNoTransitionRequest(EntityManager manager, Entity entity)
        {
            var animationStateTransitionRequest = manager.GetComponentData<AnimationStateTransitionRequest>(entity);
            Assert.IsFalse(animationStateTransitionRequest.IsValid,
                $"Expected invalid transition request, but requested is to {animationStateTransitionRequest.AnimationStateId}");
        }

        public static void AssertCurrentState(EntityManager manager, Entity entity, byte id)
        {
            var currentAnimationState = manager.GetComponentData<AnimationCurrentState>(entity);
            Assert.IsTrue(currentAnimationState.IsValid);
            Assert.AreEqual(id, currentAnimationState.AnimationStateId);
            var animationState = GetAnimationStateFromEntity(manager, entity, id);
            Assert.AreEqual(1, animationState.Weight);
        }

        public static void AssertNoOnGoingTransition(EntityManager manager, Entity entity)
        {
            AssertNoTransitionRequest(manager, entity);
            var animationStateTransition = manager.GetComponentData<AnimationStateTransition>(entity);
            Assert.IsFalse(animationStateTransition.IsValid,
                $"Expected invalid transition, but transitioning to {animationStateTransition.AnimationStateId}");
        }

        public static void AssertTransitionRequested(EntityManager manager, Entity entity, byte expectedAnimationStateId)
        {
            var animationStateTransitionRequest = manager.GetComponentData<AnimationStateTransitionRequest>(entity);
            Assert.IsTrue(animationStateTransitionRequest.IsValid);
            Assert.AreEqual(animationStateTransitionRequest.AnimationStateId, expectedAnimationStateId);
        }

        public static void AssertOnGoingTransition(EntityManager manager, Entity entity, byte expectedAnimationStateId)
        {
            var animationStateTransitionRequest = manager.GetComponentData<AnimationStateTransitionRequest>(entity);
            Assert.IsFalse(animationStateTransitionRequest.IsValid);

            var animationStateTransition = manager.GetComponentData<AnimationStateTransition>(entity);
            Assert.IsTrue(animationStateTransition.IsValid, "Expect current transition to be active");
            Assert.AreEqual(expectedAnimationStateId, animationStateTransition.AnimationStateId, $"Current transition ({animationStateTransition.AnimationStateId}) different from expected it {expectedAnimationStateId}");
        }

        internal static Entity CreateAnimationStateEntity(EntityManager manager)
        {
            var newEntity = manager.CreateEntity(
                typeof(AnimationState),
                typeof(ClipSampler));

            manager.AddComponentData(newEntity, AnimationCurrentState.Null);
            manager.AddComponentData(newEntity, AnimationStateTransitionRequest.Null);
            manager.AddComponentData(newEntity, AnimationStateTransition.Null);
            return newEntity;
        }

        internal static void SetAnimationState(EntityManager manager, Entity entity, AnimationState animation)
        {
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            var index = animationStates.IdToIndex(animation.Id);
            Assert.GreaterOrEqual(index, 0);
            animationStates[index] = animation;
        }

        internal static void SetCurrentState(EntityManager manager, Entity entity, byte animationStateId)
        {
            manager.SetComponentData(entity, new AnimationCurrentState{AnimationStateId = (sbyte) animationStateId});
            var animationState = GetAnimationStateFromEntity(manager, entity, animationStateId);
            animationState.Weight = 1;
            SetAnimationState(manager, entity, animationState);
        }
        
        internal static void TransitionTo(EntityManager manager, Entity entity, byte animationStateId,
            float transitionDuration = 0.1f)
        {
            manager.SetComponentData(entity, new AnimationStateTransitionRequest
            {
                AnimationStateId = (sbyte)animationStateId,
                TransitionDuration = transitionDuration
            });
        }

        internal static AnimationState GetAnimationStateFromEntity(EntityManager manager, Entity entity, byte animationStateId)
        {
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            return animationStates.GetWithId(animationStateId);
        }

        internal static AnimationState NewAnimationStateFromEntity(EntityManager manager, Entity entity,
            ClipSampler newSampler,
            float speed = 1, bool loop = true)
        {
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            var animationStateIndex = AnimationState.New(ref animationStates, ref samplers, newSampler, speed, loop);
            Assert.GreaterOrEqual(animationStateIndex, 0);
            Assert.IsTrue(animationStates.ExistsWithId(animationStates[animationStateIndex].Id));
            return animationStates[animationStateIndex];
        }

        internal static AnimationState NewAnimationStateFromEntity(EntityManager manager, Entity entity,
            NativeArray<ClipSampler> newSamplers,
            float speed = 1, bool loop = true)
        {
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            var animationStateIndex = AnimationState.New(ref animationStates, ref samplers, newSamplers, speed, loop);
            Assert.GreaterOrEqual(0, animationStateIndex);
            Assert.IsTrue(animationStates.ExistsWithId(animationStates[animationStateIndex].Id));
            return animationStates[animationStateIndex];
        }
        
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
            var startIndex = ClipSamplerTestUtils.AnimationStateStartSamplerIdToIndex(manager, entity, linearBlendState.AnimationStateId);
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
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);
            return LinearBlendStateUtils.NewForStateMachine(stateIndex,
                stateMachine.StateMachineBlob,
                stateMachine.ClipsBlob,
                stateMachine.ClipEventsBlob,
                ref linearBlend,
                ref animationStates,
                ref samplers
            );
        }
        
        internal static SingleClipState CreateSingleClipState(EntityManager manager, Entity entity,
            float speed = 1.0f,
            bool loop = false,
            ushort clipIndex = 0)
        {
            var singleClips = manager.GetBuffer<SingleClipState>(entity);
            var animationStates = manager.GetBuffer<AnimationState>(entity);
            var samplers = manager.GetBuffer<ClipSampler>(entity);

            var clipsBlob = CreateFakeSkeletonClipSetBlob(1);

            return SingleClipStateUtils.New(
                clipIndex, speed, loop,
                clipsBlob,
                BlobAssetReference<ClipEventsBlob>.Null,
                ref singleClips,
                ref animationStates,
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