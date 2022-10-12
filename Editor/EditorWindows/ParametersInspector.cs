using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal struct ParameterInspectorModel
    {
        internal StateMachineAsset StateMachine;
    }

    internal abstract class StateMachineInspector<T> : UnityEditor.Editor, IStateMachineInspector<T>
        where T : struct
    {
        protected T model;
        void IStateMachineInspector<T>.SetModel(T context)
        {
            model = context;
        }
    }
    internal class ParametersInspector : StateMachineInspector<ParameterInspectorModel>
    {
        public override void OnInspectorGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("Parameters", GUILayout.ExpandWidth(true));

                    var rect = EditorGUILayout.GetControlRect(GUILayout.Width(EditorGUIUtility.singleLineHeight * 2));
                    if (EditorGUI.DropdownButton(rect, new GUIContent(" +"), FocusType.Passive))
                    {
                        var menu = new GenericMenu();
                        menu.AddItem(new GUIContent("Boolean"), false, CreateParameter<BoolParameterAsset>);
                        menu.AddItem(new GUIContent("Integer"), false, CreateParameter<IntParameterAsset>);
                        menu.AddItem(new GUIContent("Float"), false, CreateParameter<FloatParameterAsset>);
                        menu.DropDown(rect);
                    }
                }

                var parametersProperty = serializedObject.FindProperty(nameof(StateMachineAsset.Parameters));
                var it = parametersProperty.GetEnumerator();
                while (it.MoveNext())
                {
                    using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                    {
                        EditorGUILayout.PropertyField(it.Current as SerializedProperty, GUIContent.none);
                    }
                }
            }

        }

        private void CreateParameter<T>()
            where T : AnimationParameterAsset
        {
            model.StateMachine.CreateParameter<T>();
            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }
    }
}