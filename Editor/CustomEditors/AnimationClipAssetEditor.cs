using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DOTSAnimation.Editor
{
    [CustomEditor(typeof(AnimationClipAsset))]
    public class AnimationClipAssetEditor : UnityEditor.Editor
    {
        public VisualTreeAsset DefaultInspector;
        private SingleClipPreview preview;
        private AnimationClipAsset ClipTarget => (AnimationClipAsset)target;

        public override VisualElement CreateInspectorGUI()
        {
            Assert.IsNotNull(preview);
            
            var inspector = new VisualElement();
            if (DefaultInspector != null)
            {
                DefaultInspector.CloneTree(inspector);
            }
            
            var defaultInspector = inspector.Q("default-inspector");
            if (defaultInspector != null)
            {
                InspectorElement.FillDefaultInspector(defaultInspector, serializedObject, this);
                var q = defaultInspector.Query<PropertyField>();
                foreach (var e in q.Build())
                {
                    e.RegisterCallback <ChangeEvent<Object>>(OnObjectPropertyChanged);
                }
            }

            // var slider = inspector.Q<Slider>("sample-time");
            // if (slider != null)
            // {
            //     slider.RegisterValueChangedCallback(OnTimeSliderValueChanged);
            //     slider.value = preview.SampleNormalizedTime;
            // }

            var previewObjField = inspector.Q<ObjectField>("preview-obj");
            if (previewObjField != null)
            {
                previewObjField.value = preview.GameObject;
                previewObjField.RegisterValueChangedCallback(OnPreviewObjectChanged);
            }

            var eventsEditor = inspector.Q<AnimationEventsEditorView>();
            if (eventsEditor != null)
            {
                eventsEditor.Initialize();
                eventsEditor.SampleTimeDragger.ValueChangedEvent += OnTimeSliderValueChanged;
                eventsEditor.SampleTimeDragger.Value = 0;
            }
            return inspector;
        }

        private void OnPreviewObjectChanged(ChangeEvent<Object> evt)
        {
            preview.GameObject = (GameObject) evt.newValue;
        }

        private void OnObjectPropertyChanged(ChangeEvent<Object> evt)
        {
            preview.Clip = ClipTarget.Clip;
        }

        private void OnTimeSliderValueChanged(float curr)
        {
            preview.SampleNormalizedTime = curr;
        }

        private void OnEnable()
        {
            preview = new SingleClipPreview(ClipTarget.Clip);
            preview.Initialize();
        }
        
        private void OnDisable()
        {
            preview?.Dispose();
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