using System;
using System.Linq;
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
                    var labelWidth = EditorGUIUtility.labelWidth;
                    var deleteButtonWidth = EditorGUIUtility.singleLineHeight;
                    var typeWidth = position.width - labelWidth - deleteButtonWidth;
                    var rects = position.HorizontalLayout(labelWidth, typeWidth, deleteButtonWidth).ToArray();

                    //label
                    {
                        parameterAsset.name = EditorGUI.TextField(rects[0], parameterAsset.name);

                        if (c.changed)
                        {
                            EditorUtility.SetDirty(parameterAsset);
                        }
                    }

                    //type
                    {
                        EditorGUI.LabelField(rects[1], $"({parameterAsset.ParameterTypeName})");
                    }

                    //delete
                    {
                        if (GUI.Button(rects[2], "-"))
                        {
                            var stateMachine = property.serializedObject.targetObject as StateMachineAsset;
                            stateMachine.DeleteParameter(parameterAsset);
                            property.serializedObject.ApplyModifiedProperties();
                            property.serializedObject.Update();
                        }
                    }
                }
            }
        }
    }
}