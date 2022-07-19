using Unity.Entities;

namespace DOTSAnimation
{
    internal struct StateMachineParameter
    {
        internal int Hash;
    }
    internal struct AnimationTransitionGroup
    {
        internal short FromStateIndex;
        internal short ToStateIndex;
        internal float NormalizedTransitionDuration;
    }
    internal struct BoolTransition
    {
        internal int GroupIndex;
        internal int ParameterIndex;
        internal bool ComparisonValue;
        internal bool Evaluate(in BoolParameter parameter)
        {
            return parameter.Value == ComparisonValue;
        }
    }
    // public struct ExitTimeTransition : IBufferElementData
    // {
    //     internal int FromStateIndex;
    //     internal int ToStateIndex;
    //     internal float NormalizedExitTime;
    //     internal bool IsTransitionToStateMachine => ToStateIndex < 0;
    // }
}