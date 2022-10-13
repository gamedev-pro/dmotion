using System;
using System.Linq;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    [CustomPropertyDrawer(typeof(TransitionCondition))]
    internal class TransitionConditionPropertyDrawer : PropertyDrawer
    {
        private ObjectReferencePopupSelector<AnimationParameterAsset> parameterPopupSelector;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (parameterPopupSelector == null)
            {
                parameterPopupSelector =
                    new SubAssetReferencePopupSelector<AnimationParameterAsset>(property.serializedObject.targetObject,
                        typeof(BoolParameterAsset),
                        typeof(IntParameterAsset));
            }

            var parameterProperty = property.FindPropertyRelative(nameof(TransitionCondition.Parameter));
            var comparisonValueProperty = property.FindPropertyRelative(nameof(TransitionCondition.ComparisonValue));
            var comparisonModeProperty = property.FindPropertyRelative(nameof(TransitionCondition.ComparisonMode));

            if (parameterProperty.objectReferenceValue == null)
            {
                parameterPopupSelector.OnGUI(position, parameterProperty, GUIContent.none);
            }
            else
            {
                var parameterAsset = (AnimationParameterAsset)parameterProperty.objectReferenceValue;
                switch (parameterAsset)
                {
                    case BoolParameterAsset:
                        DrawBoolTransitionCondition(position, parameterProperty,
                            comparisonValueProperty,
                            comparisonModeProperty);
                        break;
                    case IntParameterAsset:
                        DrawIntegerTransitionCondition(position, parameterProperty, comparisonValueProperty,
                            comparisonModeProperty);
                        break;
                    case FloatParameterAsset floatParameterAsset:
                    //Float transitions not supported
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterAsset));
                }
            }
        }

        private void DrawBoolTransitionCondition(Rect position, SerializedProperty parameterProperty,
            SerializedProperty comparisonValueProperty,
            SerializedProperty comparisonModeProperty)
        {
            var rects = position.HorizontalLayout(0.7f, 0.3f).ToArray();
            parameterPopupSelector.OnGUI(rects[0], parameterProperty, GUIContent.none);

            var enumValue = (BoolConditionComparison)EditorGUI.EnumPopup(rects[1],
                (BoolConditionComparison)comparisonModeProperty.intValue);
            comparisonModeProperty.intValue = (int)enumValue;
            comparisonValueProperty.floatValue = enumValue == BoolConditionComparison.True ? 1 : 0;
        }

        private void DrawIntegerTransitionCondition(Rect position, SerializedProperty parameterProperty,
            SerializedProperty comparisonValueProperty,
            SerializedProperty comparisonModeProperty)
        {
            var rects = position.HorizontalLayout(0.4f, 0.4f, 0.2f).ToArray();
            parameterPopupSelector.OnGUI(rects[0], parameterProperty, GUIContent.none);

            var enumValue = (IntConditionComparison)EditorGUI.EnumPopup(rects[1],
                (IntConditionComparison)comparisonModeProperty.intValue);
            comparisonModeProperty.intValue = (int)enumValue;

            comparisonValueProperty.floatValue =
                EditorGUI.IntField(rects[2], GUIContent.none, (int)comparisonValueProperty.floatValue);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}