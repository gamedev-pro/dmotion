using System;
using System.Threading;
using DMotion.Authoring;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Internal;
using UnityEngine.UIElements;

namespace DMotion.Editor
{
    internal static class ToolMenuConstants
    {
        internal const string ToolsPath = "Tools";
        internal const string DMotionPath = ToolsPath + "/DMotion";
    }

    internal struct StateMachineEditorViewModel
    {
        internal StateMachineAsset StateMachineAsset;
        internal VisualTreeAsset StateNodeXml;
        internal StateMachineInspectorView InspectorView;
        internal StateMachineInspectorView ParametersInspectorView;
    }

    internal class AnimationStateMachineEditorWindow : EditorWindow
    {
        [SerializeField] internal VisualTreeAsset StateMachineEditorXml;
        [SerializeField] internal VisualTreeAsset StateNodeXml;

        private AnimationStateMachineEditorView stateMachineEditorView;
        private StateMachineInspectorView inspectorView;
        private StateMachineInspectorView parametersInspectorView;

        [MenuItem(ToolMenuConstants.DMotionPath + "/State Machine Editor")]
        internal static void ShowExample()
        {
            var wnd = GetWindow<AnimationStateMachineEditorWindow>();
            wnd.titleContent = new GUIContent($"State Machine Editor");
            wnd.OnSelectionChange();
        }

        internal void CreateGUI()
        {
            var root = rootVisualElement;
            if (StateMachineEditorXml != null)
            {
                StateMachineEditorXml.CloneTree(root);
                stateMachineEditorView = root.Q<AnimationStateMachineEditorView>();
                inspectorView = root.Q<StateMachineInspectorView>("inspector");
                parametersInspectorView = root.Q<StateMachineInspectorView>("parameters-inspector");

                var previewElement = new VisualElement();
                previewElement.Add(new IMGUIContainer(PreviewOnInspectorGUI));
                previewElement.style.position = new StyleEnum<Position>(Position.Absolute);
                previewElement.style.bottom = new StyleLength(new Length(EditorGUIUtility.standardVerticalSpacing, LengthUnit.Pixel));
                previewElement.style.right =
                    new StyleLength(new Length(EditorGUIUtility.standardVerticalSpacing, LengthUnit.Pixel));
                previewElement.style.width = new StyleLength(new Length(300, LengthUnit.Pixel));
                previewElement.style.height = new StyleLength(new Length(300, LengthUnit.Pixel));
                root.Add(previewElement);
            }
        }

        private void OnDestroy()
        {
            if (stateMachineEditorView != null)
            {
                if (stateMachineEditorView.SingleClipPreview != null)
                    stateMachineEditorView.SingleClipPreview.Dispose();
            }
        }

        public void PreviewOnInspectorGUI()
        {
            if (stateMachineEditorView.ShouldDrawSingleClipPreview && stateMachineEditorView.SingleClipPreview != null)
            {
                var controlRect = EditorGUILayout.GetControlRect();
                controlRect.height = controlRect.width;
                stateMachineEditorView.SingleClipPreview.DrawPreview(controlRect, GUIStyle.none);
            }
        }

        [OnOpenAsset]
        internal static bool OnOpenBehaviourTree(int instanceId, int line)
        {
            if (Selection.activeObject is StateMachineAsset)
            {
                ShowExample();
                return true;
            }
            return false;
        }

        private void OnSelectionChange()
        {
            if (stateMachineEditorView != null && Selection.activeObject is StateMachineAsset stateMachine)
            {
                stateMachineEditorView.PopulateView(new StateMachineEditorViewModel()
                {
                    StateMachineAsset = stateMachine,
                    StateNodeXml = StateNodeXml,
                    InspectorView = inspectorView,
                    ParametersInspectorView = parametersInspectorView
                });
                
                //TODO (hack): UI Toolkit doesn't seem to offer *any* way to know whether the layout has been calculated
                //EditorApplication.delayedCall or VisualElement.schedule.Execute don't work, so we're stuck with this
                EditorApplication.update += WaitAndFrameAll;
                frameAllTime = EditorApplication.timeSinceStartup + 0.1f;
            }
        }

        private double frameAllTime;
        private void WaitAndFrameAll()
        {
            if (EditorApplication.timeSinceStartup > frameAllTime)
            {
                EditorApplication.update -= WaitAndFrameAll;
                stateMachineEditorView.FrameAll();
            }
        }
    }
}