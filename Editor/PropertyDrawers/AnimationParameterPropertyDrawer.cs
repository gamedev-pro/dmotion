using System;
using System.Linq;
using DMotion.Authoring;
using Unity.Entities.Editor;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    [CustomPropertyDrawer(typeof(AnimationParameterAsset))]
    internal class AnimationParameterPropertyDrawer : PropertyDrawer
    {
        private EnumTypePopupSelector enumTypePopupSelector;

        public AnimationParameterPropertyDrawer()
        {
            enumTypePopupSelector = new EnumTypePopupSelector();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var parameterAsset = property.objectReferenceValue as AnimationParameterAsset;
            var stateMachineAsset = property.serializedObject.targetObject as StateMachineAsset;
            if (parameterAsset != null && stateMachineAsset != null)
            {
                if (IsAnimatorEntitySelected(stateMachineAsset))
                {
                    DrawParameterPlaymode(position, parameterAsset, stateMachineAsset);
                }
                else
                {
                    using (new EditorGUI.DisabledScope(Application.isPlaying))
                    {
                        DrawPropertyEditorMode(position, parameterAsset, stateMachineAsset, property);
                    }
                }
            }
        }

        private static void DrawParameterPlaymode(Rect position, AnimationParameterAsset parameterAsset,
            StateMachineAsset stateMachineAsset)
        {
            var selectedEntity = (EntitySelectionProxy)Selection.activeObject;
            var rects = position.HorizontalLayout(0.7f, 0.3f).ToArray();

            //label
            EditorGUI.LabelField(rects[0], parameterAsset.name);

            //value
            {
                switch (parameterAsset)
                {
                    case BoolParameterAsset:
                    {
                        var parameterIndex = stateMachineAsset.Parameters.OfType<BoolParameterAsset>()
                            .FindIndex(p => parameterAsset == p);
                        var boolParameter = selectedEntity.GetBuffer<BoolParameter>()[parameterIndex];
                        EditorGUI.Toggle(rects[1], GUIContent.none, boolParameter.Value);
                        break;
                    }
                    case IntParameterAsset:
                    {
                        var parameterIndex = stateMachineAsset.Parameters.OfType<IntParameterAsset>()
                            .FindIndex(p => parameterAsset == p);
                        var intParameter = selectedEntity.GetBuffer<IntParameter>()[parameterIndex];

                        if (parameterAsset is EnumParameterAsset enumParameterAsset)
                        {
                            EditorGUIUtils.GenericEnumPopup(rects[1],
                                enumParameterAsset.EnumType.Type,
                                intParameter.Value);
                        }
                        else
                        {
                            EditorGUI.IntField(rects[1], GUIContent.none, intParameter.Value);
                        }

                        break;
                    }
                    case FloatParameterAsset:
                    {
                        var parameterIndex = stateMachineAsset.Parameters.OfType<FloatParameterAsset>()
                            .FindIndex(p => parameterAsset == p);
                        var floatParameter = selectedEntity.GetBuffer<BlendParameter>()[parameterIndex];
                        EditorGUI.FloatField(rects[1], GUIContent.none, floatParameter.Value);
                        break;
                    }
                    default:
                        throw new NotImplementedException(
                            $"No handling for type {parameterAsset.GetType().Name}");
                }
            }
        }

        private bool IsAnimatorEntitySelected(StateMachineAsset myStateMachineAsset)
        {
            return Application.isPlaying && Selection.activeObject is EntitySelectionProxy entitySelectionProxy &&
                   entitySelectionProxy.Exists && entitySelectionProxy.HasComponent<AnimationStateMachineDebug>()
                   && entitySelectionProxy.GetManagedComponent<AnimationStateMachineDebug>().StateMachineAsset ==
                   myStateMachineAsset;
        }

        private void DrawPropertyEditorMode(Rect position, AnimationParameterAsset parameterAsset,
            StateMachineAsset stateMachine,
            SerializedProperty property)
        {
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                var deleteButtonWidth = EditorGUIUtility.singleLineHeight;
                var typeWidth = position.width - labelWidth - deleteButtonWidth;
                var rects = position.HorizontalLayout(labelWidth, typeWidth, deleteButtonWidth).ToArray();

                //label
                {
                    var newName = EditorGUI.DelayedTextField(rects[0], parameterAsset.name);

                    if (newName != parameterAsset.name)
                    {
                        parameterAsset.name = newName;
                        EditorUtility.SetDirty(parameterAsset);
                        AssetDatabase.SaveAssetIfDirty(parameterAsset);
                        AssetDatabase.Refresh();
                    }
                }

                //type
                {
                    if (parameterAsset is EnumParameterAsset enumParameterAsset)
                    {
                        enumTypePopupSelector.DrawSelectionPopup(rects[1],
                            GUIContent.none,
                            enumParameterAsset.EnumType.Type,
                            newType =>
                            {
                                enumParameterAsset.EnumType.Type = newType;
                                EditorUtility.SetDirty(enumParameterAsset);
                            });
                    }
                    else
                    {
                        EditorGUI.LabelField(rects[1], $"({parameterAsset.ParameterTypeName})");
                    }
                }

                //delete
                {
                    if (GUI.Button(rects[2], "-"))
                    {
                        stateMachine.DeleteParameter(parameterAsset);
                        property.serializedObject.ApplyModifiedProperties();
                        property.serializedObject.Update();
                    }
                }
            }
        }
    }
}