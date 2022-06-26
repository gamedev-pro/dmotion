using DOTSAnimation;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

[BurstCompile]
public static class AnimationEvents
{
    public static unsafe FunctionPointer<AnimationEventDelegate> StartAttackWindowEventPtr =>
        BurstCompiler.CompileFunctionPointer<AnimationEventDelegate>(StartAttackWindowEvent);
    public static unsafe FunctionPointer<AnimationEventDelegate> EndAttackWindowEventPtr =>
        BurstCompiler.CompileFunctionPointer<AnimationEventDelegate>(EndAttackWindowEvent);
    public static unsafe FunctionPointer<AnimationEventDelegate> FootStepEventPtr =>
        BurstCompiler.CompileFunctionPointer<AnimationEventDelegate>(FootStepEvent);
    public static unsafe FunctionPointer<AnimationEventDelegate> FootStepEvent2Ptr =>
        BurstCompiler.CompileFunctionPointer<AnimationEventDelegate>(FootStepEvent2);
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(AnimationEventDelegate))]
    public static unsafe void StartAttackWindowEvent(Entity* owner, int sortKey, EntityCommandBuffer.ParallelWriter* ecb)
    {
        ecb->SetComponent(sortKey, *owner, new AttackWindow(){ IsOpen = true });
    }
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(AnimationEventDelegate))]
    public static unsafe void EndAttackWindowEvent(Entity* owner, int sortKey, EntityCommandBuffer.ParallelWriter* ecb)
    {
        ecb->SetComponent(sortKey, *owner, new AttackWindow(){ IsOpen = false });
    }
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(AnimationEventDelegate))]
    public static unsafe void FootStepEvent(Entity* owner, int sortKey, EntityCommandBuffer.ParallelWriter* ecb)
    {
        Debug.Log("FOOTSTEP");
    }
    
    [BurstCompile]
    [AOT.MonoPInvokeCallback(typeof(AnimationEventDelegate))]
    public static unsafe void FootStepEvent2(Entity* owner, int sortKey, EntityCommandBuffer.ParallelWriter* ecb)
    {
        Debug.Log("FOOTSTEP 2");
    }
}