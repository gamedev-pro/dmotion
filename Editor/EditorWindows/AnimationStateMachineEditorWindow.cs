using System;
using DMotion.Authoring;
using Unity.Entities.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
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
        internal EntitySelectionProxy SelectedEntity;
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
            wnd.titleContent = new GUIContent("State Machine Editor");
            wnd.OnSelectionChange();
        }

        private void Awake()
        {
            EditorApplication.playModeStateChanged += OnPlaymodeStateChanged;
        }


        private void OnDestroy()
        {
            EditorApplication.playModeStateChanged -= OnPlaymodeStateChanged;
        }

        private void OnPlaymodeStateChanged(PlayModeStateChange stateChange)
        {
            switch (stateChange)
            {
                case PlayModeStateChange.EnteredEditMode:
                    stateMachineEditorView?.UpdateView();
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(stateChange), stateChange, null);
            }
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

        #if UNITY_EDITOR || DEBUG
        private void Update()
        {
            if (Application.isPlaying && stateMachineEditorView != null)
            {
                stateMachineEditorView.UpdateView();
            }
        }
        #endif

        private void OnSelectionChange()
        {
            if (stateMachineEditorView != null && !Application.isPlaying &&
                Selection.activeObject is StateMachineAsset stateMachineAsset)
            {
                stateMachineEditorView.PopulateView(new StateMachineEditorViewModel
                {
                    StateMachineAsset = stateMachineAsset,
                    StateNodeXml = StateNodeXml,
                    InspectorView = inspectorView,
                    ParametersInspectorView = parametersInspectorView
                });
                WaitAndFrameAll();
            }

            //
            if (stateMachineEditorView != null && Application.isPlaying &&
                Selection.activeObject is EntitySelectionProxy entitySelectionProxy)
            {
                if (entitySelectionProxy.HasComponent<AnimationStateMachineDebug>())
                {
                    var stateMachineDebug = entitySelectionProxy.GetManagedComponent<AnimationStateMachineDebug>();

                    stateMachineEditorView.PopulateView(new StateMachineEditorViewModel
                    {
                        StateMachineAsset = stateMachineDebug.StateMachineAsset,
                        SelectedEntity = entitySelectionProxy,
                        StateNodeXml = StateNodeXml,
                        InspectorView = inspectorView,
                        ParametersInspectorView = parametersInspectorView
                    });
                    WaitAndFrameAll();
                }
            }
        }

        private void WaitAndFrameAll()
        {
            //TODO (hack): UI Toolkit doesn't seem to offer *any* way to know whether the layout has been calculated
            //EditorApplication.delayedCall or VisualElement.schedule.Execute don't work, so we're stuck with this
            EditorApplication.update += DoFrameAll;
            frameAllTime = EditorApplication.timeSinceStartup + 0.1f;
        }

        private double frameAllTime;

        private void DoFrameAll()
        {
            if (EditorApplication.timeSinceStartup > frameAllTime)
            {
                EditorApplication.update -= DoFrameAll;
                stateMachineEditorView.FrameAll();
            }
        }
    }
}