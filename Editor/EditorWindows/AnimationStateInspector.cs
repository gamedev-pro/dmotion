using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal class SingleStateInspector : AnimationStateInspector
    {
        private SerializedProperty clipProperty;
        protected override void OnEnable()
        {
            base.OnEnable();
            clipProperty = serializedObject.FindProperty(nameof(SingleClipStateAsset.Clip));
        }

        protected override void DrawChildProperties()
        {
            EditorGUILayout.PropertyField(clipProperty);
        }
    }
    
    internal class LinearBlendStateInspector : AnimationStateInspector
    {
        private SerializedProperty blendParameterProperty;
        private SerializedProperty clipsProperty;
        private ObjectReferencePopupSelector<FloatParameterAsset> blendParametersSelector;

        protected override void OnEnable()
        {
            base.OnEnable();
            blendParameterProperty = serializedObject.FindProperty(nameof(LinearBlendStateAsset.BlendParameter));
            clipsProperty = serializedObject.FindProperty(nameof(LinearBlendStateAsset.BlendClips));
            blendParametersSelector =
                new SubAssetReferencePopupSelector<FloatParameterAsset>(blendParameterProperty.serializedObject
                    .targetObject);
        }

        protected override void DrawChildProperties()
        {
            blendParametersSelector.OnGUI(EditorGUILayout.GetControlRect(), blendParameterProperty, new GUIContent(blendParameterProperty.displayName));
            EditorGUILayout.PropertyField(clipsProperty);
        }
    }

    internal struct AnimationStateInspectorModel
    {
        internal StateNodeView StateView;
        internal AnimationStateAsset StateAsset => StateView.State;
    }

    internal abstract class AnimationStateInspector : StateMachineInspector<AnimationStateInspectorModel>
    {
        private SerializedProperty loopProperty;
        private SerializedProperty speedProperty;
        private SerializedProperty outTransitionsProperty;

        protected virtual void OnEnable()
        {
            loopProperty = serializedObject.FindProperty(nameof(SingleClipStateAsset.Loop));
            speedProperty = serializedObject.FindProperty(nameof(SingleClipStateAsset.Speed));
            outTransitionsProperty = serializedObject.FindProperty(nameof(SingleClipStateAsset.OutTransitions));
        }

        public override void OnInspectorGUI()
        {
            DrawName();
            DrawLoopProperty();
            DrawSpeedProperty();
            DrawChildProperties();
            DrawTransitions();
        }

        protected abstract void DrawChildProperties();

        protected void DrawName()
        {
            using (var c = new EditorGUI.ChangeCheckScope())
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Name", GUILayout.Width(EditorGUIUtility.labelWidth));
                model.StateAsset.name = EditorGUILayout.TextField(model.StateAsset.name);
                if (c.changed)
                {
                    model.StateView.title = model.StateAsset.name;
                }
            }
        }

        protected void DrawLoopProperty()
        {
            EditorGUILayout.PropertyField(loopProperty);
        }
        protected void DrawSpeedProperty()
        {
            EditorGUILayout.PropertyField(speedProperty);
        }

        protected void DrawTransitions()
        {
            if (model.StateAsset.OutTransitions.Count == 0)
            {
                return;
            }

            EditorGUILayout.LabelField(outTransitionsProperty.displayName);
            foreach (var transition in model.StateAsset.OutTransitions)
            {
                StateMachineEditorUtils.DrawTransitionSummary(model.StateAsset, transition.ToState, transition.TransitionDuration);
            }
        }
    }
}