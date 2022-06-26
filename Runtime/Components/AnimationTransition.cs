using Unity.Entities;

namespace DOTSAnimation
{
    public struct AnimationTransitionGroup : IBufferElementData
    {
        internal int FromStateIndex;
        internal int ToStateIndex;
    }
    
    public struct BoolTransition : IBufferElementData
    {
        internal int GroupIndex;
        internal int ParameterIndex;
        internal bool ComparisonValue;
        internal bool Evaluate(in BoolParameter parameter)
        {
            return parameter.Value == ComparisonValue;
        }
    }

    public struct ExitTimeTransition : IBufferElementData
    {
        internal int FromStateIndex;
        internal int ToStateIndex;
        internal float NormalizedExitTime;
        internal bool IsTransitionToStateMachine => ToStateIndex < 0;
    }
}