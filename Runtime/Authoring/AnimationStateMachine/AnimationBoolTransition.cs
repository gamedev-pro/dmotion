using System;

namespace DMotion.Authoring
{
    [Serializable]
    public class AnimationBoolTransition
    {
        [SubAssetsOnly]
        public BoolParameterAsset Parameter;
        public bool ComparisonValue;
    }
}