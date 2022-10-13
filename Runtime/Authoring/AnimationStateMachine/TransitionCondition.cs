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
            ComparisonMode = (IntConditionComparison)ComparisonMode,
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

        public TransitionCondition ToGenericCondition()
        {
            return new TransitionCondition
            {
                Parameter = BoolParameter,
                ComparisonValue = (float)ComparisonValue,
                ComparisonMode = (int)ComparisonValue
            };
        }
    }

    public struct IntegerTransitionCondition
    {
        public IntParameterAsset IntParameter;
        public IntConditionComparison ComparisonMode;
        public int ComparisonValue;
        public TransitionCondition ToGenericCondition()
        {
            return new TransitionCondition
            {
                Parameter = IntParameter,
                ComparisonValue = ComparisonValue,
                ComparisonMode = (int) ComparisonMode
            };
        }
    }
}