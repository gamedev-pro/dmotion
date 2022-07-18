using Unity.Entities;

namespace DOTSAnimation
{
    public struct StateMachineBlob
    {
        internal BlobArray<AnimationStateBlob> States;
        internal BlobArray<SingleClipStateBlob> SingleClipStates;
        // internal BlobArray<LinearBlendStateBlob> LinearBlendStates;
        
        internal BlobArray<AnimationTransitionGroup> Transitions;
        internal BlobArray<StateMachineParameter> Parameters;
        
        //Array of array of events. Maps directly to SkeletonClipSetBlob.Clips that this StateMachine uses
        internal BlobArray<AnimationClipEventsBlob> ClipEvents;
    }
}