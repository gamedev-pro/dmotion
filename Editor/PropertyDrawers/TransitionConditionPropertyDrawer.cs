using System;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal enum BoolConditionModes
    {
        True,
        False
    }
    
    [CustomPropertyDrawer(typeof(TransitionCondition))]
    internal class TransitionConditionPropertyDrawer : PropertyDrawer
    {
        private ObjectReferencePopupSelector<AnimationParameterAsset> parameterPopupSelector;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (parameterPopupSelector == null)
            {
                parameterPopupSelector = new SubAssetReferencePopupSelector<AnimationParameterAsset>(property.serializedObject.targetObject, typeof(BoolParameterAsset));
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
                    case BoolParameterAsset boolParameterAsset:
                        var labelRect = position;
                        labelRect.width *= 0.7f;
                        parameterPopupSelector.OnGUI(labelRect, parameterProperty, GUIContent.none);
                        
                        var comparisonModeRect = position;
                        comparisonModeRect.xMin += labelRect.width + EditorGUIUtility.standardVerticalSpacing;
                        var enumValue = (BoolConditionModes) EditorGUI.EnumPopup(comparisonModeRect, (BoolConditionModes)comparisonModeProperty.intValue);
                        comparisonModeProperty.intValue = (int)enumValue;
                        comparisonValueProperty.floatValue = enumValue == BoolConditionModes.True ? 1 : 0;
                        break;
                    case FloatParameterAsset floatParameterAsset:
                        //Float transitions not supported
                    default:
                        throw new ArgumentOutOfRangeException(nameof(parameterAsset));
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}