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
        //Max 2 states transitioning at a time, max 3 *active* clips per state, +1 clip for one shot
        internal const int kMaxSamplerCount = 2 * 3 + 1;
        internal BlobAssetReference<SkeletonClipSetBlob> ClipsBlob;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal AnimationState CurrentState;
        internal AnimationState NextState;
        internal StateTransition CurrentTransition;
        
        //TODO (perf): Do those get inlined? It's just syntax sugar
        internal readonly ref AnimationTransitionGroup CurrentTransitionBlob =>
            ref StateMachineBlob.Value.Transitions[CurrentTransition.TransitionIndex];
        internal readonly ref BlobArray<AnimationTransitionGroup> TransitionsBlob => ref StateMachineBlob.Value.Transitions;

        internal AnimationState CreateState(short stateIndex)
        {
            return new AnimationState()
            {
                Clips = ClipsBlob,
                StateMachineBlob = StateMachineBlob,
                StateIndex = stateIndex,
                NormalizedTime = 0,
            };
        }
    }

    public struct BoolParameter : IBufferElementData
    {
        public int Hash;
        public bool Value;
    }
    
    public struct BlendParameter : IBufferElementData
    {
        public int Hash;
        public int StateIndex;
        public float Value;
    }
}