using DOTSAnimation;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;

public static class AnimationEvents
{
    public static int StartAttackEventHash = "StartAttackEvent".GetHashCode();
    public static int EndAttackEventHash = "EndAttackEvent".GetHashCode();
    public static int FootstepEvent1 = "FootstepEvent1".GetHashCode();
    public static int FootstepEvent2 = "FootstepEvent2".GetHashCode();
}
public struct CombatAnimations : IComponentData
{
    public AnimationStateRef Attack;
    public AnimationStateRef Block;
}

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class PlayOneShotExampleAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IRequestBlobAssets
{
    public GameObject Owner;
    public AnimationClip AttackClip;
    public AnimationClip BlockClip;
    
    private SmartBlobberHandle<SkeletonClipSetBlob> clipBlob;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var blob = clipBlob.Resolve();

        var atkState = AnimationConversionUtils.AddAnimationState_Single(entity, dstManager, blob, 0,
            ClipSamplerCreateParams.Default, AnimationStateCreateParams.OneShot, "Slash");
        var blockState = AnimationConversionUtils.AddAnimationState_Single(entity, dstManager, blob, 1,
            ClipSamplerCreateParams.Default, AnimationStateCreateParams.OneShot, "Heavy Atk");
        
        atkState.AddEvent(0.2f, AnimationEvents.StartAttackEventHash);
        atkState.AddEvent(0.35f, AnimationEvents.EndAttackEventHash);

        var combatAnimations = new CombatAnimations()
        {
            Attack = atkState.ToStateRef(),
            Block = blockState.ToStateRef(),
        };

        dstManager.AddComponentData(entity, combatAnimations);
    }

    public void RequestBlobAssets(Entity entity, EntityManager dstEntityManager, GameObjectConversionSystem conversionSystem)
    {
        var clips = new[]
        {
            AttackClip,
            BlockClip
        };
        clipBlob = AnimationConversionUtils.RequestBlobAssets(clips, gameObject, conversionSystem);
    }
}