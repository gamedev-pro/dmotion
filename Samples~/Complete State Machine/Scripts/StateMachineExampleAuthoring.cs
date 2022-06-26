using DOTSAnimation;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;
using static DOTSAnimation.AnimationConversionUtils;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class StateMachineExampleAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
{
    public GameObject owner;
    public AnimationClip idleClip;
    public AnimationClip walkClip;
    public AnimationClip runClip;
    public AnimationClip jumpClip;

    private SmartBlobberHandle<SkeletonClipSetBlob> clipBlob;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var blob = clipBlob.Resolve();

        //create samplers
        var idleSampler =
            AddSampler(entity, dstManager, blob, 0, new ClipSamplerCreateParams(1, 0));
        var walkSampler =
            AddSampler(entity, dstManager, blob, 1, new ClipSamplerCreateParams(1, 0.2f));
        var runSampler =
            AddSampler(entity, dstManager, blob, 2, new ClipSamplerCreateParams(1, 0.8f));

        //Add events
        walkSampler.AddEvent(0.2f, AnimationEvents.FootStepEventPtr);
        runSampler.AddEvent(0.1f, AnimationEvents.FootStepEvent2Ptr);

        //Create 1D Blend Tree state
        var locomotionState = AnimationConversionUtils.AddAnimationState(entity, dstManager, idleSampler.Index,
            runSampler.Index, new AnimationStateCreateParams(AnimationSamplerType.LinearBlend, 0.3f), "Locomotion");
        //Add blend tree parameter
        AddBlendParameter(locomotionState, "Speed");

        // Create single clip state
        var jumpState = AddAnimationState_Single(entity, dstManager, blob, 3,
            ClipSamplerCreateParams.Default, AnimationStateCreateParams.DefaultLoop, "Jump");

        //Two boolean parameters
        var isJumpingParam = AddParameter<bool>(entity, dstManager, "IsJumping");
        var isFallingParam = AddParameter<bool>(entity, dstManager, "IsFalling");

        //Transition from Locomotion -> Jump (both isJumping and isFalling need to be true)
        var transitionGroup = AddTransitionGroup(locomotionState, jumpState);
        AddTransition(transitionGroup, isJumpingParam, true);
        AddTransition(transitionGroup, isFallingParam, true);
        
        //Transition from Jump -> Locomotion when isJumping is false
        AddSingleTransition(jumpState, locomotionState, isJumpingParam, false);

        //Create fms
        AddAnimationStateMachine(entity,dstManager,conversionSystem, owner, locomotionState);
    }

    public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager,
        GameObjectConversionSystem conversionSystem)
    {
        var clips = new[]
        {
            idleClip, walkClip, runClip, jumpClip
        };
        clipBlob = AnimationConversionUtils.RequestBlobAssets(clips, gameObject, conversionSystem);
    }
}