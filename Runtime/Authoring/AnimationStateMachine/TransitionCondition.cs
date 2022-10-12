using System;

namespace DMotion.Authoring
{
    [Serializable]
    public struct TransitionCondition
    {
        public AnimationParameterAsset Parameter;
        public float ComparisonValue;
        public int ComparisonMode;

        public BoolTransitionCondition AsBoolCondition => new()
        {
            BoolParameter = (BoolParameterAsset)Parameter,
            ComparisonValue = (BoolConditionModes)ComparisonMode
        };

        public IntegerTransitionCondition AsIntegerCondition => new()
        {
            IntParameter = (IntParameterAsset)Parameter,
            Comparison = (IntegerConditionComparison)ComparisonMode,
            ComparisonValue = (int)ComparisonValue
        };
    }

    public enum BoolConditionModes
    {
        True,
        False
    }

    public struct BoolTransitionCondition
    {
        public BoolParameterAsset BoolParameter;
        public BoolConditionModes ComparisonValue;
    }

    public enum IntegerConditionComparison
    {
        Equal,
        NotEqual,
        Greater,
        Less,
        GreaterOrEqual,
        LessOrEqual
    }

    public struct IntegerTransitionCondition
    {
        public IntParameterAsset IntParameter;
        public IntegerConditionComparison Comparison;
        public int ComparisonValue;
    }
}