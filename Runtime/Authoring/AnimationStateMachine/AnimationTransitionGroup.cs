using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace DMotion.Authoring
{
    [Serializable]
    public class StateOutTransition
    {
        public AnimationStateAsset ToState;
        public bool HasEndTime;
        [Min(0)]
        public float EndTime;
        [Min(0), FormerlySerializedAs("NormalizedTransitionDuration")]
        public float TransitionDuration;
        public List<TransitionCondition> Conditions;

        public IEnumerable<TransitionCondition> BoolTransitions =>
            Conditions.Where(c => c.Parameter is BoolParameterAsset);

        public StateOutTransition(AnimationStateAsset to,
            float transitionDuration = 0.15f, List<TransitionCondition> boolTransitions = null)
        {
            ToState = to;
            TransitionDuration = transitionDuration;
            Conditions = boolTransitions ?? new List<TransitionCondition>();
        }
    }
}