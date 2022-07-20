using Latios.Kinemation;
using Unity.Entities;

namespace DOTSAnimation
{
    internal struct StateTransition
    {
        internal short TransitionIndex;
        internal bool IsValid => TransitionIndex >= 0;
        internal static StateTransition Null => new() { TransitionIndex = -1 };
    }
    
    internal struct AnimationStateMachine : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> ClipsBlob;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal AnimationState CurrentState;
        internal AnimationState NextState;
        internal StateTransition CurrentTransition;
        
        //TODO (perf): Do those get inlined? It's just syntax sugar
        internal readonly ref AnimationTransitionGroup CurrentTransitionBlob =>
            ref StateMachineBlob.Value.Transitions[CurrentTransition.TransitionIndex];
        internal readonly ref BlobArray<AnimationTransitionGroup> TransitionsBlob => ref StateMachineBlob.Value.Transitions;

    }

    public struct BoolParameter : IBufferElementData
    {
        public int Hash;
        public bool Value;
    }
    
    public struct BlendParameter : IBufferElementData
    {
        public int Hash;
        public float Value;
    }
}