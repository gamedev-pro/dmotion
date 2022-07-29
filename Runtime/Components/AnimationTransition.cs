using Unity.Entities;

namespace DMotion
{
    internal struct StateOutTransitionGroup
    {
        internal short ToStateIndex;
        internal float NormalizedTransitionDuration;
        internal float TransitionEndTime;
        internal BlobArray<BoolTransition> BoolTransitions;
        internal bool HasEndTime => TransitionEndTime > 0;
        internal bool HasAnyConditions => BoolTransitions.Length > 0;
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