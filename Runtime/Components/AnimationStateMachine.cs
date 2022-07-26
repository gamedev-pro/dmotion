using Latios.Kinemation;
using Unity.Entities;

namespace DMotion
{
    internal struct AnimationStateMachine : IComponentData
    {
        internal BlobAssetReference<SkeletonClipSetBlob> ClipsBlob;
        internal BlobAssetReference<ClipEventsBlob> ClipEventsBlob;
        internal BlobAssetReference<StateMachineBlob> StateMachineBlob;
        internal AnimationState CurrentState;
        internal AnimationState NextState;
        internal StateTransition CurrentTransition;
        internal float Weight;
        
        //TODO (perf): Do those get inlined? It's just syntax sugar
        internal readonly ref AnimationTransitionGroup CurrentTransitionBlob =>
            ref StateMachineBlob.Value.Transitions[CurrentTransition.TransitionIndex];
    }
}