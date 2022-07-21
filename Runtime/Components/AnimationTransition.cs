using Unity.Entities;

namespace DOTSAnimation
{
    internal struct StateTransition
    {
        internal short TransitionIndex;
        internal bool IsValid => TransitionIndex >= 0;
        internal static StateTransition Null => new StateTransition() { TransitionIndex = -1 };
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
}