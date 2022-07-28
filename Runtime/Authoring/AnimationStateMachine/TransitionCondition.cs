using System;

namespace DMotion.Authoring
{
    [Serializable]
    public class TransitionCondition
    {
        public AnimationParameterAsset Parameter;
        public float ComparisonValue;
        public int ComparisonMode;
    }
}