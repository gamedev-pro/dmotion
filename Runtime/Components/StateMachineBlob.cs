using Unity.Entities;

namespace DOTSAnimation
{
    public struct StateMachineBlob
    {
        internal BlobArray<AnimationStateBlob> States;
        internal BlobArray<SingleClipStateBlob> SingleClipStates;
        // internal BlobArray<LinearBlendStateBlob> LinearBlendStates;
        
        internal BlobArray<StateMachineParameter> Parameters;
        
        internal BlobArray<AnimationClipEvent> ClipEvents;
        
        internal BlobArray<AnimationTransitionGroup> Transitions;
        internal BlobArray<BoolTransition> BoolTransitions;
    }
}