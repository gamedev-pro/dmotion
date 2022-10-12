using System;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    [CustomPropertyDrawer(typeof(AnimationParameterAsset))]
    internal class AnimationParameterPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var parameterAsset = property.objectReferenceValue as AnimationParameterAsset;
            if (parameterAsset != null)
            {
                using (var c = new EditorGUI.ChangeCheckScope())
                {
                    var labelRect = position;
                    labelRect.width = EditorGUIUtility.labelWidth;
                    parameterAsset.name = EditorGUI.TextField(labelRect, parameterAsset.name);

                    if (c.changed)
                    {
                        EditorUtility.SetDirty(parameterAsset);
                    }
                    
                    var deleteButtonRect = position;
                    deleteButtonRect.xMin = deleteButtonRect.xMax - EditorGUIUtility.singleLineHeight;
                    if (GUI.Button(deleteButtonRect, "-"))
                    {
                        var stateMachine = property.serializedObject.targetObject as StateMachineAsset;
                        stateMachine.DeleteParameter(parameterAsset);
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    }

                    var typeRect = position;
                    typeRect.xMax -= deleteButtonRect.width - EditorGUIUtility.standardVerticalSpacing;
                    typeRect.xMin += labelRect.width + EditorGUIUtility.standardVerticalSpacing*3;
                    EditorGUI.LabelField(typeRect, $"({parameterAsset.ParameterTypeName})");
                }
            }
        }
    }
}