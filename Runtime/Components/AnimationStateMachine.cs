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
        internal float Weight;
        internal float CurrentTransitionNormalizedTime;
    }
}