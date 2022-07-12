using System.Collections.Generic;
using System.Linq;
using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTSAnimation.Editor
{
    //not using attribute here, this property drawer needs to be instantiated by custom editors
    public class AnimationEventsPropertyDrawer : PropertyDrawer
    {
        private static Texture2D timelineDragTexture;

        static AnimationEventsPropertyDrawer()
        {
            timelineDragTexture = new Texture2D(1, 1);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.PropertyField(property, label, true);
            position = EditorGUILayout.GetControlRect();
            GUI.color = Color.red;
            GUI.DrawTexture(position, timelineDragTexture);
        }
    }
    public class AnimationEventsEditorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AnimationEventsEditorView, UxmlTraits>{}

        private const string ButtonAddEvent = "button-add-event";
        private const string TimeDragger = "dragger-container";
        private const string DragArea = "unity-drag-container";
        private const string EventsProperty = "property-events";

        public SliderDragger SampleTimeDragger;
        private AnimationClipAsset clipAsset;
        private SerializedObject serializedObject;

        public void Initialize(AnimationClipAsset animationClipAsset, SerializedObject serializedObject)
        {
            clipAsset = animationClipAsset;
            this.serializedObject = serializedObject;
            var timeDraggerElement = this.Q<VisualElement>(TimeDragger);
            var dragAreaElement = this.Q<VisualElement>(DragArea);
            SampleTimeDragger = new SliderDragger(timeDraggerElement, dragAreaElement);

            var button = this.Q<Button>(ButtonAddEvent);
            button.clicked += OnAddEventClicked;

            var eventsProperty = this.Q<PropertyField>(EventsProperty);
            eventsProperty.BindProperty(serializedObject.FindProperty(nameof(AnimationClipAsset.Events)));
            eventsProperty.schedule.Execute(RegisterToListChangedEvent).ExecuteLater(1000);
        }

        private void RegisterToListChangedEvent()
        {
            var eventsProperty = this.Q<PropertyField>(EventsProperty);
            var listView = eventsProperty.Q<ListView>();
            listView.itemsAdded += OnItemsAdded;
            listView.itemsRemoved += OnItemsRemoved;
        }

        private void OnItemsRemoved(IEnumerable<int> obj)
        {
            Debug.Log("REMOVE");
        }

        private void OnItemsAdded(IEnumerable<int> obj)
        {
            Debug.Log("HERE");
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
                clipAsset.Events = clipAsset.Events.Append(newEvent).ToArray();
                
            }
        }
    }
}