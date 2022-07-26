using Unity.Entities;

namespace DMotion.Samples.CompleteStateMachine
{
    [GenerateAuthoringComponent]
    public struct AttackWindow : IComponentData
    {
        public bool IsOpen;
    }
}