using System;
using System.Collections;
using System.Collections.Generic;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal class SingleStateInspector : AnimationStateInspector
    {
        private SerializedProperty clipProperty;
        private AnimationEventsPropertyDrawer eventsPropertyDrawer;
        
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
            DrawAnimationClip(childValue, ref eventsPropertyDrawer);
        }
    }
    
    internal class LinearBlendStateInspector : AnimationStateInspector
    {
        private SerializedProperty blendParameterProperty;
        private SerializedProperty clipsProperty;
        private ObjectReferencePopupSelector<FloatParameterAsset> blendParametersSelector;

        private AnimationEventsPropertyDrawer[] drawerArray = Array.Empty<AnimationEventsPropertyDrawer>();

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
            
            // EditorGUILayout.PropertyField(clipsProperty);
            if (clipsProperty != null)
            {
                clipsProperty.isExpanded = EditorGUILayout.Foldout(clipsProperty.isExpanded, "BlendClips");
                if (clipsProperty.isExpanded)
                {
                    clipsProperty.arraySize = EditorGUILayout.IntField("Size", clipsProperty.arraySize);
                    
                    if (clipsProperty.arraySize > 0)
                        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    for (var i = 0; i < clipsProperty.arraySize; i += 1)
                    {
                        var childClipWithThresholdProp = clipsProperty.GetArrayElementAtIndex(i);
                        var clipReferencePath = childClipWithThresholdProp.FindPropertyRelative(nameof(DMotion.Authoring
                            .ClipWithThreshold
                            .Clip));

                        if (clipReferencePath.objectReferenceValue == null)
                        {
                            // We don't have a clip here yet
                            using (var c = new EditorGUI.ChangeCheckScope())
                            {
                                var clipRef = (AnimationClip) EditorGUILayout.ObjectField(null, typeof(AnimationClip), false);

                                if (c.changed && clipRef != null)
                                {
                                    clipReferencePath.objectReferenceValue = model.StateView.StateMachine.CreateClipAsset(clipRef);
                                    serializedObject.ApplyModifiedProperties();
                                }
                            }
                            
                            if (i != clipsProperty.arraySize - 1)
                                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                            continue;
                        }
                        
                        var childClipAsset =
                            (AnimationClipAsset) clipReferencePath.objectReferenceValue;
                        var threshold =
                            childClipWithThresholdProp.FindPropertyRelative(nameof(DMotion.Authoring.ClipWithThreshold
                                .Threshold));

                        AnimationEventsPropertyDrawer tempEventDrawer = drawerArray.Length > i ? drawerArray[i] : null;
                        DrawAnimationClip(childClipAsset, ref tempEventDrawer, () =>
                        {
                            EditorGUILayout.PropertyField(threshold);
                        });
                        if (drawerArray.Length <= i)
                            Array.Resize(ref drawerArray, i + 1);
                        drawerArray[i] = tempEventDrawer;
                        if (i != clipsProperty.arraySize - 1)
                            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
                    }
                }
            }
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

        protected void DrawAnimationClip(AnimationClipAsset childValue, ref AnimationEventsPropertyDrawer eventsPropertyDrawer, Action drawExtra = null)
        {
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
                        if (model.Preview != null && model.Preview is SingleClipPreview singleClipPreview)
                        {
                            if (model.Preview.GameObject != model.StateView.StateMachine.ClipPreviewGameObject)
                                model.Preview.GameObject = model.StateView.StateMachine.ClipPreviewGameObject;
                            singleClipPreview.Clip = childValue.Clip;
                            if (eventsPropertyDrawer != null)
                            {
                                // Detatch the previous preview from the single preview pane so we can use the new
                                // one.
                                if (model.StateView.ParentView.lastUsedDrawer != null)
                                    model.StateView.ParentView.lastUsedDrawer.SetPreview(null);
                                eventsPropertyDrawer.SetPreview(singleClipPreview);
                                model.StateView.ParentView.lastUsedDrawer = eventsPropertyDrawer;
                                model.Preview.Initialize();
                                model.StateView.ParentView.ShouldDrawSingleClipPreview = true;
                            }
                        }
                    }
                }
                GUILayout.EndHorizontal();

                var drawerRect = EditorGUILayout.GetControlRect();
                drawerRect.xMax -= 60;
                
                if (eventsPropertyDrawer == null)
                    eventsPropertyDrawer = new AnimationEventsPropertyDrawer(
                    childValue, 
                    nestedSerializedObject.FindProperty(nameof(AnimationClipAsset.Events)),
                    null);
                eventsPropertyDrawer.OnInspectorGUI(drawerRect);
                
                drawExtra?.Invoke();

                if (c.changed)
                {
                    nestedSerializedObject.ApplyModifiedProperties();
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }
    }
}