using Unity.Entities;

namespace DMotion
{
    internal struct StateTransition
    {
        internal short TransitionIndex;
        internal bool IsValid => TransitionIndex >= 0;
        internal static StateTransition Null => new StateTransition() { TransitionIndex = -1 };
    }
    
    internal struct StateOutTransitionGroup
    {
        internal short ToStateIndex;
        internal float NormalizedTransitionDuration;
        internal BlobArray<BoolTransition> BoolTransitions;
    }
    internal struct BoolTransition
    {
        internal int ParameterIndex;
        internal bool ComparisonValue;
        internal bool Evaluate(in BoolParameter parameter)
        {
            return parameter.Value == ComparisonValue;
        }
    }
}