using System;
using System.Collections.Generic;
using System.Linq;
using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTSAnimation.Editor
{
    public class AnimationEventsEditorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AnimationEventsEditorView, UxmlTraits>{}

        private VisualTreeAsset eventMarkerXml;

        private const string ButtonAddEvent = "button-add-event";
        private const string TimeDragger = "dragger-container";
        private const string DragArea = "unity-drag-container";

        private SliderDragger SampleTimeDragger;
        private AnimationClipAsset clipAsset;

        public Action<float> SampleTimeChanged;

        private List<SliderDragger> eventMarkers = new();
        private VisualElement dragArea;

        public void Initialize(AnimationClipAsset animationClipAsset, SerializedObject serializedObject, VisualTreeAsset eventMarkerTemplate)
        {
            clipAsset = animationClipAsset;
            eventMarkerXml = eventMarkerTemplate;
            var timeDraggerElement = this.Q<VisualElement>(TimeDragger);
            dragArea = this.Q<VisualElement>(DragArea);
            SampleTimeDragger = new SliderDragger(timeDraggerElement, dragArea);
            SampleTimeDragger.ValueChangedEvent += OnSampleTimeChanged;

            var button = this.Q<Button>(ButtonAddEvent);
            button.clicked += OnAddEventClicked;
            
            var eventsPropertyField = new ArrayPropertyField(serializedObject.FindProperty(nameof(AnimationClipAsset.Events)));
            eventsPropertyField.ArrayChanged += OnEventsArrayChanged;
            Add(eventsPropertyField);

            CreateEventMarkers();
        }

		private void CreateEventMarkers()
        {
            if (eventMarkerXml == null)
            {
                return;
            }
            DestroyEventMarkers();
            for (var i = 0; i < clipAsset.Events.Length; i++)
            {
                var e = clipAsset.Events[i];
                eventMarkerXml.CloneTree(dragArea);
                var eventMarker = dragArea.Children().Last();
                var dragger = new SliderDragger(eventMarker, dragArea);
                dragger.Value = e.NormalizedTime;

                var index = i;
                dragger.ValueChangedEvent += v => OnEventMarkerMove(v, index);
                eventMarkers.Add(dragger);
            }
        }

        private void OnEventMarkerMove(float normalizedValue, int eventIndex)
        {
            clipAsset.Events[eventIndex].NormalizedTime = normalizedValue;
            EditorUtility.SetDirty(clipAsset);
            SampleTimeChanged?.Invoke(normalizedValue);
        }

        private void DestroyEventMarkers()
        {
            foreach (var marker in eventMarkers)
            {
                marker.target.RemoveFromHierarchy();
            }
            eventMarkers.Clear();
        }

        private void OnSampleTimeChanged(float normalizedTime)
        {
            //This needsd to be encapsulated as this event will be called when "Event makers" are moved as well
            SampleTimeChanged?.Invoke(normalizedTime);
        }

        private void OnEventsArrayChanged()
        {
            CreateEventMarkers();
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