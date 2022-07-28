using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal struct TransitionGroupInspectorModel
    {
        internal AnimationStateAsset FromState;
        internal AnimationStateAsset ToState;
    }
    
    internal class TransitionGroupInspector : StateMachineInspector<TransitionGroupInspectorModel>
    {
        private SerializedProperty outTransitionsProperty;

        private void OnEnable()
        {
            outTransitionsProperty =
                serializedObject.FindProperty(nameof(AnimationStateAsset.OutTransitions));
        }

        public override void OnInspectorGUI()
        {
            var it = outTransitionsProperty.GetEnumerator();
            while (it.MoveNext())
            {
                var outTransitionProperty = (SerializedProperty)it.Current;
                var toStateProperty = outTransitionProperty.FindPropertyRelative(nameof(StateOutTransition.ToState));
                if (toStateProperty.objectReferenceValue == model.ToState)
                {
                    var hasEndTimeProperty =
                        outTransitionProperty.FindPropertyRelative(nameof(StateOutTransition.HasEndTime));
                    
                    var endTimeProperty =
                        outTransitionProperty.FindPropertyRelative(nameof(StateOutTransition.EndTime));
                    
                    var normalizedTimeProperty =
                        outTransitionProperty.FindPropertyRelative(nameof(StateOutTransition.NormalizedTransitionDuration));

                    var conditionsProperty =
                        outTransitionProperty.FindPropertyRelative(nameof(StateOutTransition.Conditions));

                    StateMachineEditorUtils.DrawTransitionSummary(
                        model.FromState,
                        toStateProperty.objectReferenceValue as AnimationStateAsset,
                        normalizedTimeProperty.floatValue);

                    EditorGUILayout.PropertyField(hasEndTimeProperty);
                    if (hasEndTimeProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(endTimeProperty);
                    }
                    EditorGUILayout.PropertyField(normalizedTimeProperty, new GUIContent("Duration (%)"));
                    EditorGUILayout.PropertyField(conditionsProperty);
                }
            }
        }
    }
}