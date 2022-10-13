using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DMotion.Authoring
{
    public struct AnimationStateMachineAssetBuilder
    {
        private StateMachineAsset stateMachineAsset;

        public static AnimationStateMachineAssetBuilder New()
        {
            var stateMachineAsset = ScriptableObject.CreateInstance<StateMachineAsset>();
            stateMachineAsset.States = new List<AnimationStateAsset>();
            stateMachineAsset.Parameters = new List<AnimationParameterAsset>();
            return new AnimationStateMachineAssetBuilder
                { stateMachineAsset = stateMachineAsset };
        }

        public StateMachineAsset Build()
        {
            return stateMachineAsset;
        }

        public T AddState<T>(bool loop = true, float speed = 1)
            where T : AnimationStateAsset
        {
            var state = ScriptableObject.CreateInstance<T>();
            state.Loop = loop;
            state.Speed = speed;
            state.OutTransitions = new List<StateOutTransition>();
            stateMachineAsset.States.Add(state);

            if (stateMachineAsset.DefaultState == null)
            {
                stateMachineAsset.DefaultState = state;
            }
            return state;
        }

        public T AddParameter<T>(string name)
            where T : AnimationParameterAsset
        {
            var parameter = ScriptableObject.CreateInstance<T>();
            parameter.name = name;
            stateMachineAsset.Parameters.Add(parameter);
            return parameter;
        }

        public StateOutTransition AddTransition(AnimationStateAsset from, AnimationStateAsset to)
        {
            from.OutTransitions.Add(new StateOutTransition(to));
            return from.OutTransitions.Last();
        }

        public void AddIntCondition(StateOutTransition outTransition, IntParameterAsset intParameter,
            IntConditionComparison comparison, int comparisonValue)
        {
            var intTransitionCondition = new IntegerTransitionCondition
            {
                IntParameter = intParameter,
                ComparisonMode = comparison,
                ComparisonValue = comparisonValue
            };
            outTransition.Conditions.Add(intTransitionCondition.ToGenericCondition());
        }
    }
}