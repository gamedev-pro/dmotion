using Unity.Entities;

[GenerateAuthoringComponent]
public struct AttackWindow : IComponentData
{
    public bool IsOpen;
}