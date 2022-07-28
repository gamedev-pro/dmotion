using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DMotion.Authoring
{
    [Serializable]
    public class StateOutTransition
    {
        public AnimationStateAsset ToState;
        public bool HasEndTime;
        [Range(0,1)]
        public float EndTime;
        [Range(0,1)]
        public float NormalizedTransitionDuration;
        public List<TransitionCondition> Conditions;

        public IEnumerable<TransitionCondition> BoolTransitions =>
            Conditions.Where(c => c.Parameter is BoolParameterAsset);

        public StateOutTransition(AnimationStateAsset to,
            float transitionDuration = 0.15f, List<TransitionCondition> boolTransitions = null)
        {
            ToState = to;
            NormalizedTransitionDuration = transitionDuration;
            Conditions = boolTransitions ?? new List<TransitionCondition>();
        }
    }
}