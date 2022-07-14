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
        private static readonly Texture2D whiteTex;
        private static Texture2D eventMarkerTex;

        private static Texture2D EventMarkerTex
        {
            get
            {
                if (eventMarkerTex != null)
                {
                    return eventMarkerTex;
                }

                eventMarkerTex = (Texture2D) EditorGUIUtility.Load("Animation.EventMarker");
                eventMarkerTex.filterMode = FilterMode.Bilinear;
                return eventMarkerTex;
            }
        }

        static AnimationEventsPropertyDrawer()
        {
            whiteTex = new Texture2D(1, 1);
        }
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var clipAsset = (AnimationClipAsset) property.serializedObject.targetObject;
            var eventMarkers = new List<Rect>(clipAsset.Events.Length);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.LabelField(position, label);

                var dragRect = position;
                dragRect.x += 60;
                dragRect.height = GetPropertyHeight(property, label);
                DrawEventsDragArea(dragRect, clipAsset, eventMarkers);
            }
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(property, GUIContent.none, true);
        }

        private void DrawEventsDragArea(Rect area, AnimationClipAsset clipAsset, List<Rect> eventMarkers)
        {
            {
                var dragAreaRect = area;
                var dragAreaColor = Color.black;
                dragAreaColor.a = 0.2f;
                GUI.color = dragAreaColor;
                GUI.DrawTexture(dragAreaRect, whiteTex);
            }
            {
                var bottomBarRect = area;
                var barHeight = bottomBarRect.height * 0.1f;
                bottomBarRect.y = bottomBarRect.y + bottomBarRect.height - barHeight;
                bottomBarRect.height = barHeight;
                GUI.color = Color.gray;
                GUI.DrawTexture(bottomBarRect, whiteTex);
            }
            {
                var markerRect = area;
                markerRect.width = 2;
                GUI.color = Color.red;
                GUI.DrawTexture(markerRect, whiteTex);
            }
            {
                eventMarkers.Clear();
                for (var i = 0; i < clipAsset.Events.Length; i++)
                {
                    var e = clipAsset.Events[i];
                    var eventMarkerRect = area;
                    eventMarkerRect.width = 10;
                    eventMarkerRect.height *= 0.9f;
                    eventMarkerRect.x = NormalizedTimeToPixels(e.NormalizedTime, eventMarkerRect, area);
                    GUI.color = Color.white;
                    GUI.DrawTexture(eventMarkerRect, EventMarkerTex);
                    eventMarkers.Add(eventMarkerRect);
                }
            }
        }

        private float NormalizedTimeToPixels(float time, in Rect rect, in Rect parentRect)
        {
            return parentRect.xMin + (parentRect.width - rect.width)* time;
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