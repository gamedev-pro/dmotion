using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DOTSAnimation.Editor
{
    [CustomEditor(typeof(AnimationClipAsset))]
    public class AnimationClipAssetEditor : UnityEditor.Editor
    {
        public VisualTreeAsset EditorXml;
        private SingleClipPreview preview;
        private AnimationClipAsset ClipTarget => (AnimationClipAsset)target;
        
        private void OnEnable()
        {
            preview = new SingleClipPreview(ClipTarget.Clip);
            preview.Initialize();
        }
        
        private void OnDisable()
        {
            preview?.Dispose();
        }

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();
            if (EditorXml != null)
            {
                EditorXml.CloneTree(inspector);
                var objField = inspector.Q<ObjectField>("object-preview");
                objField.value = preview.GameObject;
                objField.RegisterValueChangedCallback(OnPreviewObjectChanged);
                
                var eventsEditorView = inspector.Q<AnimationEventsEditorView>();
                eventsEditorView.Initialize(ClipTarget, serializedObject);
                eventsEditorView.SampleTimeChanged += OnSampleTimeChanged;
            }
            
            return inspector;
        }

        private void OnSampleTimeChanged(float normalizedTime)
        {
            preview.SampleNormalizedTime = normalizedTime;
        }

        private void OnPreviewObjectChanged(ChangeEvent<Object> evt)
        {
            preview.GameObject = evt.newValue as GameObject;
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            preview.DrawPreview(r, background);
        }
    }
}