using System;
using System.Collections.Generic;
using System.Linq;
using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTSAnimation.Editor
{
    [CustomPropertyDrawer(typeof(AnimationClipEvent))]
    public class AnimationEventsPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var inspector = new VisualElement();
            inspector.Add(new PropertyField(property.FindPropertyRelative(nameof(AnimationClipEvent.Name))));
            inspector.Add(new PropertyField(property.FindPropertyRelative(nameof(AnimationClipEvent.NormalizedTime))));
            return inspector;
        }
    }
    public class AnimationEventsEditorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AnimationEventsEditorView, UxmlTraits>{}

        private const string ButtonAddEvent = "button-add-event";
        private const string TimeDragger = "dragger-container";
        private const string DragArea = "unity-drag-container";

        private SliderDragger SampleTimeDragger;
        private AnimationClipAsset clipAsset;

        public Action<float> SampleTimeChanged;

        public void Initialize(AnimationClipAsset animationClipAsset, SerializedObject serializedObject)
        {
            clipAsset = animationClipAsset;
            var timeDraggerElement = this.Q<VisualElement>(TimeDragger);
            var dragAreaElement = this.Q<VisualElement>(DragArea);
            SampleTimeDragger = new SliderDragger(timeDraggerElement, dragAreaElement);
            SampleTimeDragger.ValueChangedEvent += OnSampleTimeChanged;

            var button = this.Q<Button>(ButtonAddEvent);
            button.clicked += OnAddEventClicked;
            
            var eventsPropertyField = new ArrayPropertyField(serializedObject.FindProperty(nameof(AnimationClipAsset.Events)));
            eventsPropertyField.ArrayChanged += OnEventsArrayChanged;
            Add(eventsPropertyField);
        }

        private void OnSampleTimeChanged(float normalizedTime)
        {
            //This needsd to be encapsulated as this event will be called when "Event makers" are moved as well
            SampleTimeChanged?.Invoke(normalizedTime);
        }

        private void OnEventsArrayChanged()
        {
            Debug.Log(clipAsset.Events.Length);
        }

        private void OnAddEventClicked()
        {
            if (clipAsset != null)
            {
                var newEvent = new AnimationClipEvent()
                {
                    Name = $"New Event {clipAsset.Events.Length}",
                    NormalizedTime = SampleTimeDragger.Value
                };
                Undo.RecordObject(clipAsset, "Add Event");
                clipAsset.Events = clipAsset.Events.Append(newEvent).ToArray();
                EditorUtility.SetDirty(clipAsset);
            }
        }
    }
}