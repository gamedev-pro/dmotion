using Unity.Entities;

namespace DOTSAnimation
{
    public struct StateMachineBlob
    {
        internal short DefaultStateIndex;
        internal BlobArray<AnimationStateBlob> States;
        internal BlobArray<SingleClipStateBlob> SingleClipStates;
        internal BlobArray<LinearBlendStateBlob> LinearBlendStates;
        
        // internal BlobArray<StateMachineParameter> Parameters;
        // internal BlobArray<StateMachineParameter> BoolParameters;
        
        internal BlobArray<AnimationClipEvent> ClipEvents;
        
        internal BlobArray<AnimationTransitionGroup> Transitions;
        internal BlobArray<BoolTransition> BoolTransitions;
    }
}