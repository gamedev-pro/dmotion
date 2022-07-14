using System;
using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DOTSAnimation.Editor
{
    public class ArrayPropertyField : PropertyField
    {
        public Action ArrayChanged;
        private SerializedProperty property;
        private int prevArraySize;

        public ArrayPropertyField(SerializedProperty prop) : base(prop)
        {
            property = prop;
            Assert.IsTrue(property.isArray);
            prevArraySize = property.arraySize;
        }
        
        protected override void ExecuteDefaultActionAtTarget(EventBase evt)
        {
            base.ExecuteDefaultActionAtTarget(evt);
            if (evt is GeometryChangedEvent && property.arraySize != prevArraySize)
            {
                prevArraySize = property.arraySize;
                ArrayChanged?.Invoke();
            }
        }
    }
    [CustomEditor(typeof(AnimationClipAsset))]
    public class AnimationClipAssetEditor : UnityEditor.Editor
    {
        public VisualTreeAsset EventsTimelineAsset;
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
            var objField = new ObjectField("Preview Object");
            objField.value = preview.GameObject;
            objField.objectType = typeof(GameObject);
            objField.allowSceneObjects = true;
            objField.RegisterValueChangedCallback(OnPreviewObjectChanged);
            inspector.Add(objField);
            
            var clipProperty = serializedObject.FindProperty(nameof(AnimationClipAsset.Clip));
            inspector.Add(new PropertyField(clipProperty));

            if (EventsTimelineAsset != null)
            {
                EventsTimelineAsset.CloneTree(inspector);
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