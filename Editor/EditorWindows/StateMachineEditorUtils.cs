using System;
using System.Linq;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Editor
{
    internal static class StateMachineEditorUtils
    {
        public static AnimationStateAsset CreateState(this StateMachineAsset stateMachineAsset, Type type)
        {
            Assert.IsTrue(typeof(AnimationStateAsset).IsAssignableFrom(type));
            var state = ScriptableObject.CreateInstance(type) as AnimationStateAsset;
            Assert.IsNotNull(state);

            state.name = $"New State {stateMachineAsset.States.Count + 1}";
            state.StateEditorData.Guid = GUID.Generate().ToString();
            //TODO: Enable this later. Create editor tool to change this as well
            // state.hideFlags = HideFlags.HideInHierarchy;
            
            Undo.RecordObject(stateMachineAsset, $"{stateMachineAsset.name}: Create State");
            stateMachineAsset.States.Add(state);

            if (stateMachineAsset.DefaultState == null)
            {
                stateMachineAsset.SetDefaultState(state);
            }
            AssetDatabase.AddObjectToAsset(state, stateMachineAsset);
            Undo.RegisterCreatedObjectUndo(state, $"{stateMachineAsset.name}: Create State");
            
            AssetDatabase.SaveAssets();
            return state;
        }

        public static void DeleteState(this StateMachineAsset stateMachineAsset, AnimationStateAsset stateAsset)
        {
            //Remove all transitions that reference this state
            foreach (var state in stateMachineAsset.States)
            {
                for (int i = state.OutTransitions.Count - 1; i >= 0; i--)
                {
                    var transition = state.OutTransitions[i];
                    if (transition.ToState == stateAsset)
                    {
                        state.OutTransitions.RemoveAt(i);
                    }
                }
            }
            stateMachineAsset.States.Remove(stateAsset);
            Undo.DestroyObjectImmediate(stateAsset);
            AssetDatabase.SaveAssets();
        }

        public static AnimationParameterAsset CreateParameter<T>(this StateMachineAsset stateMachineAsset)
            where T : AnimationParameterAsset
        {
            return stateMachineAsset.CreateParameter(typeof(T));
        }
        public static AnimationParameterAsset CreateParameter(this StateMachineAsset stateMachineAsset, Type type)
        {
            Assert.IsTrue(typeof(AnimationParameterAsset).IsAssignableFrom(type));
            var parameter = ScriptableObject.CreateInstance(type) as AnimationParameterAsset;
            Assert.IsNotNull(parameter);

            parameter.name = $"New Parameter {stateMachineAsset.Parameters.Count + 1}";
            //TODO: Enable this later. Create editor tool to change this as well
            // state.hideFlags = HideFlags.HideInHierarchy;
            
            Undo.RecordObject(stateMachineAsset, $"{stateMachineAsset.name}: Create State");
            stateMachineAsset.Parameters.Add(parameter);
            AssetDatabase.AddObjectToAsset(parameter, stateMachineAsset);
            Undo.RegisterCreatedObjectUndo(parameter, $"{stateMachineAsset.name}: Create State");
            
            AssetDatabase.SaveAssets();
            return parameter;
        }
        
        public static void DeleteParameter(this StateMachineAsset stateMachineAsset, AnimationParameterAsset parameterAsset)
        {
            Undo.RecordObject(stateMachineAsset, $"{stateMachineAsset.name}: Delete Parameter {parameterAsset.name}");
            //Remove all transitions that reference this state
            foreach (var state in stateMachineAsset.States)
            {
                for (int i = state.OutTransitions.Count - 1; i >= 0; i--)
                {
                    var transition = state.OutTransitions[i];
                    transition.Conditions.RemoveAll(c => c.Parameter == parameterAsset);
                }
            }

            stateMachineAsset.Parameters.Remove(parameterAsset);
            Undo.DestroyObjectImmediate(parameterAsset);
            AssetDatabase.SaveAssets();
        }

        public static bool IsDefaultState(this StateMachineAsset stateMachineAsset, AnimationStateAsset state)
        {
            return stateMachineAsset.DefaultState == state;
        }

        public static void SetDefaultState(this StateMachineAsset stateMachineAsset, AnimationStateAsset state)
        {
            Assert.IsTrue(stateMachineAsset.States.Contains(state), $"State {state.name} not present in State machine {stateMachineAsset.name}");
            stateMachineAsset.DefaultState = state;
        }
        
        internal static void DrawTransitionSummary(AnimationStateAsset fromState, AnimationStateAsset toState, float transitionTime)
        {
            var labelWidth = Mathf.Min(EditorGUIUtility.labelWidth, 80f);
            using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField($"{fromState.name}", GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField("--->", GUILayout.Width(40f));
                EditorGUILayout.LabelField($"{toState.name}", GUILayout.Width(labelWidth));
                EditorGUILayout.LabelField($"({transitionTime}%)", GUILayout.Width(50f));
            }
        }
    }
}