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
        public override void OnInspectorGUI()
        {
            var outTransitionsProperty =
                serializedObject.FindProperty(nameof(AnimationStateAsset.OutTransitions));
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
                    
                    var transitionDurationProperty =
                        outTransitionProperty.FindPropertyRelative(nameof(StateOutTransition.TransitionDuration));

                    var conditionsProperty =
                        outTransitionProperty.FindPropertyRelative(nameof(StateOutTransition.Conditions));

                    StateMachineEditorUtils.DrawTransitionSummary(
                        model.FromState,
                        toStateProperty.objectReferenceValue as AnimationStateAsset,
                        transitionDurationProperty.floatValue);

                    EditorGUILayout.PropertyField(hasEndTimeProperty);
                    if (hasEndTimeProperty.boolValue)
                    {
                        EditorGUILayout.PropertyField(endTimeProperty);
                    }
                    EditorGUILayout.PropertyField(transitionDurationProperty, new GUIContent("Duration (s)"));
                    EditorGUILayout.PropertyField(conditionsProperty);
                }
            }
        }
    }
}