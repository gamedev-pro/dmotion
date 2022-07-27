using System;
using System.Collections.Generic;

namespace DMotion.Authoring
{
    [Serializable]
    public class StateOutTransition
    {
        [SubAssetsOnly]
        public AnimationStateAsset ToState;
        public float NormalizedTransitionDuration;
        public List<AnimationBoolTransition> BoolTransitions;

        public StateOutTransition(AnimationStateAsset to,
            float transitionDuration = 0.15f, List<AnimationBoolTransition> boolTransitions = null)
        {
            ToState = to;
            NormalizedTransitionDuration = transitionDuration;
            BoolTransitions = boolTransitions ?? new List<AnimationBoolTransition>();
        }
    }
}