using System;
using Unity.Entities;

namespace DMotion
{
    internal struct StateOutTransitionGroup
    {
        internal short ToStateIndex;
        internal float TransitionDuration;
        internal float TransitionEndTime;
        internal BlobArray<BoolTransition> BoolTransitions;
        internal BlobArray<IntTransition> IntTransitions;
        internal bool HasEndTime => TransitionEndTime > 0;
        internal bool HasAnyConditions => BoolTransitions.Length > 0 || IntTransitions.Length > 0;
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

    public enum IntConditionComparison
    {
        Equal = 0,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }
    internal struct IntTransition
    {
        internal int ParameterIndex;
        internal IntConditionComparison ComparisonMode;
        internal int ComparisonValue;
        internal bool Evaluate(in IntParameter parameter)
        {
            return ComparisonMode switch
            {
                IntConditionComparison.Equal => parameter.Value == ComparisonValue,
                IntConditionComparison.NotEqual => parameter.Value != ComparisonValue,
                IntConditionComparison.Greater => parameter.Value > ComparisonValue,
                IntConditionComparison.Less => parameter.Value < ComparisonValue,
                IntConditionComparison.GreaterOrEqual => parameter.Value >= ComparisonValue,
                IntConditionComparison.LessOrEqual => parameter.Value <= ComparisonValue,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}