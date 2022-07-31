using System;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Editor
{
    internal static class StateMachineEditorUtils
    {
        public static AnimationClipAsset CreateClipAsset(this StateMachineAsset stateMachineAsset, AnimationClip clipReference)
        {
            var clip = ScriptableObject.CreateInstance<AnimationClipAsset>();

            clip.name = $"New Clip Asset {stateMachineAsset.States.Count + 1}";
            clip.Clip = clipReference;
            stateMachineAsset.Clips.Add(clip);
            AssetDatabase.AddObjectToAsset(clip, stateMachineAsset);
            
            AssetDatabase.SaveAssets();
            return clip;
        }

        public static void DeleteClipAsset(this StateMachineAsset stateMachineAsset, AnimationClipAsset clipAsset)
        {
            //Remove all transitions that reference this state
            stateMachineAsset.Clips.Remove(clipAsset);
            AssetDatabase.RemoveObjectFromAsset(clipAsset);
            AssetDatabase.SaveAssets();
        }
        
        public static AnimationStateAsset CreateState(this StateMachineAsset stateMachineAsset, Type type)
        {
            Assert.IsTrue(typeof(AnimationStateAsset).IsAssignableFrom(type));
            var state = ScriptableObject.CreateInstance(type) as AnimationStateAsset;
            Assert.IsNotNull(state);

            state.name = $"New State {stateMachineAsset.States.Count + 1}";
            state.StateEditorData.Guid = GUID.Generate().ToString();
            //TODO: Enable this later. Create editor tool to change this as well
            // state.hideFlags = HideFlags.HideInHierarchy;
            
            stateMachineAsset.States.Add(state);

            if (stateMachineAsset.DefaultState == null)
            {
                stateMachineAsset.SetDefaultState(state);
            }
            AssetDatabase.AddObjectToAsset(state, stateMachineAsset);
            
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
            AssetDatabase.RemoveObjectFromAsset(stateAsset);
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
            
            stateMachineAsset.Parameters.Add(parameter);
            AssetDatabase.AddObjectToAsset(parameter, stateMachineAsset);
            
            AssetDatabase.SaveAssets();
            return parameter;
        }
        
        public static void DeleteParameter(this StateMachineAsset stateMachineAsset, AnimationParameterAsset parameterAsset)
        {
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
            AssetDatabase.RemoveObjectFromAsset(parameterAsset);
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