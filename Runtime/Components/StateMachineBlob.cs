using Unity.Entities;

namespace DOTSAnimation
{
    public struct StateMachineBlob
    {
        internal BlobArray<SingleClipStateBlob> SingleClipStates;
        // internal BlobArray<LinearBlendStateBlob> LinearBlendStates;
        internal BlobArray<StateMachineParameter> Parameters;
        internal BlobArray<AnimationStateBlob> States;
        internal BlobArray<AnimationTransitionGroup> Transitions;
        internal BlobArray<AnimationEventBlob> Events;
    }
}