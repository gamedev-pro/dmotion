using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal class SingleStateInspector : AnimationStateInspector
    {
        private SerializedProperty clipProperty;
        private AnimationEventsPropertyDrawer _eventsPropertyDrawer;
        
        protected override void OnEnable()
        {
            base.OnEnable();
            clipProperty = serializedObject.FindProperty(nameof(SingleClipStateAsset.Clip));
        }

        protected override void DrawChildProperties()
        {
            if (clipProperty.objectReferenceValue == null)
            {
                using (var c = new EditorGUI.ChangeCheckScope())
                {
                    var clipRef = (AnimationClip) EditorGUILayout.ObjectField(null, typeof(AnimationClip), false);

                    if (c.changed && clipRef != null)
                    {
                        clipProperty.objectReferenceValue = model.StateView.StateMachine.CreateClipAsset(clipRef);
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                return;
            }

            var childValue = (AnimationClipAsset) clipProperty.objectReferenceValue;
            var nestedSerializedObject = new SerializedObject(childValue);
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                GUILayout.BeginHorizontal();
                {
                    var controlRect = EditorGUILayout.GetControlRect();
                    var copyRect = new Rect(controlRect);
                    copyRect.width -= (controlRect.height + EditorGUIUtility.standardVerticalSpacing);

                    childValue.Clip =
                        (AnimationClip)EditorGUI.ObjectField(copyRect, childValue.Clip, typeof(AnimationClip), true);

                    controlRect.width = controlRect.height;
                    controlRect.x += copyRect.width + EditorGUIUtility.standardVerticalSpacing;
                    if (GUI.Button(controlRect, PlayClipContent, PlayClipStyle))
                    {
                        Debug.Log("Pressing play");
                        if (model.Preview != null && model.Preview is SingleClipPreview singleClipPreview)
                        {
                            if (model.Preview.GameObject != model.StateView.StateMachine.ClipPreviewGameObject)
                                model.Preview.GameObject = model.StateView.StateMachine.ClipPreviewGameObject;
                            singleClipPreview.Clip = childValue.Clip;
                            if (_eventsPropertyDrawer != null)
                            {
                                // Detatch the previous preview from the single preview pane so we can use the new
                                // one.
                                if (model.StateView.ParentView._lastUsedDrawer != null)
                                    model.StateView.ParentView._lastUsedDrawer.SetPreview(null);
                                _eventsPropertyDrawer.SetPreview(singleClipPreview);
                                model.StateView.ParentView._lastUsedDrawer = _eventsPropertyDrawer;
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();
                // TODO add a play button or something on this to let it change hte preview

                var drawerRect = EditorGUILayout.GetControlRect();
                drawerRect.xMax -= 60;
                
                if (_eventsPropertyDrawer == null)
                    _eventsPropertyDrawer = new AnimationEventsPropertyDrawer(
                    childValue, 
                    nestedSerializedObject.FindProperty(nameof(AnimationClipAsset.Events)),
                    null);
                _eventsPropertyDrawer.OnInspectorGUI(drawerRect);

                if (c.changed)
                {
                    nestedSerializedObject.ApplyModifiedProperties();
                    serializedObject.ApplyModifiedProperties();
                }
            }
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

        internal PlayableGraphPreview Preview;
    }

    internal abstract class AnimationStateInspector : StateMachineInspector<AnimationStateInspectorModel>
    {
        private SerializedProperty loopProperty;
        private SerializedProperty speedProperty;
        private SerializedProperty outTransitionsProperty;

        private static GUIStyle addRemoveEventStyle;
        private static GUIContent addEventContent;

        protected static GUIStyle PlayClipStyle
        {
            get
            {
                if (addRemoveEventStyle == null)
                {
                    addRemoveEventStyle = new GUIStyle(EditorStyles.miniButton)
                    {
                        fixedHeight = 0,
                        padding = new RectOffset(-1, 1, 0, 0)
                    };
                }

                return addRemoveEventStyle;
            }
        }

        protected static GUIContent PlayClipContent
        {
            get
            {
                if (addEventContent == null)
                {
                    addEventContent = EditorGUIUtility.IconContent("Animation.Play", "Play Clip");
                }

                return addEventContent;
            }
        }
        
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
                StateMachineEditorUtils.DrawTransitionSummary(model.StateAsset, transition.ToState, transition.NormalizedTransitionDuration);
            }
        }
    }
}