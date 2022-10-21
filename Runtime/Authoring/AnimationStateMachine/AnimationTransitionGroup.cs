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

        public IEnumerable<BoolTransitionCondition> BoolTransitions =>
            Conditions.Where(c => c.Parameter is BoolParameterAsset).Select(c => c.AsBoolCondition);
        public IEnumerable<IntegerTransitionCondition> IntTransitions =>
            Conditions.Where(c => c.Parameter is IntParameterAsset).Select(c => c.AsIntegerCondition);

        public StateOutTransition(AnimationStateAsset to,
            float transitionDuration = 0.15f,
            List<BoolTransitionCondition> boolTransitions = null,
            List<IntegerTransitionCondition> intTransitions = null)
        {
            ToState = to;
            TransitionDuration = transitionDuration;
            Conditions = new List<TransitionCondition>();
            if (boolTransitions != null)
            {
                Conditions.AddRange(boolTransitions.Select(b => b.ToGenericCondition()));
            }

            if (intTransitions != null)
            {
                Conditions.AddRange(intTransitions.Select(i => i.ToGenericCondition()));
            }
        }
    }
}