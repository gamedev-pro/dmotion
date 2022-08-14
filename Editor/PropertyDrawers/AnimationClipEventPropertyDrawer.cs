using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    [CustomPropertyDrawer(typeof(Authoring.AnimationClipEvent))]
    internal class AnimationClipEventPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.height = EditorGUIUtility.singleLineHeight;
            //Draw event name
            {
                var eventNameProperty = property.FindPropertyRelative(nameof(Authoring.AnimationClipEvent.Name));
                EditorGUI.PropertyField(position, eventNameProperty);
            }

            {
                var normalizedTimeProperty = property.FindPropertyRelative(nameof(Authoring.AnimationClipEvent.NormalizedTime));
                var clipAsset = property.serializedObject.targetObject as AnimationClipAsset;
                var isClipValid = clipAsset != null && clipAsset.Clip != null;
                
                var length = isClipValid ? clipAsset.Clip.length : 1.0f;
                
                var normalizedTime = Mathf.Clamp01(normalizedTimeProperty.floatValue);

                //draw time slider
                {
                    position.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    var labelRect = position;
                    labelRect.width = EditorGUIUtility.labelWidth;
                    EditorGUI.LabelField(labelRect, "Time");

                    var sliderRect = position;
                    sliderRect.xMin += labelRect.width + EditorGUIUtility.standardVerticalSpacing;
                    var time = EditorGUI.Slider(sliderRect, normalizedTime*length, 0.0f, length);
                    normalizedTimeProperty.floatValue = time / length;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 2 + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}