using System.Collections.Generic;
using System.Linq;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal class AnimationEventsPropertyDrawer
    {
        private static Texture2D whiteTex;
        private static Texture2D eventMarkerTex;
        private static GUIStyle addRemoveEventStyle;
        private static GUIContent addEventContent;
        private static GUIContent removeEventContent;

        private static Color selectedEventColor = new Color32(56, 196, 235, 255);//light blue
        private static Color unselectedEventColor = Color.white;

        private Rect dragArea;
        private RectElement timeMarker;
        private float timeMarkerTime;
        private List<RectElement> eventMarkers = new List<RectElement>();
        private bool isDraggingTimeMarker = false;
        private int eventMarkerDragIndex = -1;
        
        private readonly SingleClipPreview preview;
        private readonly SerializedProperty property;
        private readonly AnimationClipAsset clipAsset;

        private static Texture2D WhiteTex
        {
            get
            {
                if (whiteTex == null)
                {
                    whiteTex = new Texture2D(1, 1);
                }

                return whiteTex;
            }
        }
        private static Texture2D EventMarkerTex
        {
            get
            {
                if (eventMarkerTex == null)
                {
                    eventMarkerTex = (Texture2D) EditorGUIUtility.Load("Animation.EventMarker");
                    eventMarkerTex.filterMode = FilterMode.Bilinear;
                }
                return eventMarkerTex;
            }
        }

        private static GUIStyle AddRemoveEventStyle
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

        private static GUIContent RemoveEventContent
        {
            get
            {
                if (removeEventContent == null)
                {
                    removeEventContent = new GUIContent("X");
                }
                return removeEventContent;
            }
        }

        private static GUIContent AddEventContent
        {
            get
            {
                if (addEventContent == null)
                {
                    addEventContent = EditorGUIUtility.IconContent("Animation.AddEvent", "Add event");
                }

                return addEventContent;
            }
        }
        public AnimationEventsPropertyDrawer(AnimationClipAsset clipAsset, SerializedProperty property, SingleClipPreview preview)
        {
            this.clipAsset = clipAsset;
            this.preview = preview;
            this.property = property;
            timeMarkerTime = preview.SampleTime;
        }


        public void OnInspectorGUI(Rect area)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.LabelField(area, property.displayName);

                var addEventButtonWidth = area.height;//square button
                dragArea = area;
                dragArea.x += 60;
                dragArea.xMax -= (addEventButtonWidth + EditorGUIUtility.standardVerticalSpacing);
                DrawEventsDragArea(dragArea);

                var addEventButtonRect = area;
                addEventButtonRect.width = addEventButtonWidth;
                addEventButtonRect.x = dragArea.xMax + EditorGUIUtility.standardVerticalSpacing;
                DrawAddRemoveEventButton(addEventButtonRect);
            }

            using (var c = new EditorGUI.ChangeCheckScope())
            {
                var cachedEvents = clipAsset.Events.ToList();
                GUI.color = Color.white;
                EditorGUILayout.PropertyField(property, GUIContent.none, true);
                if (c.changed)
                {
                    ClearSelection();
                    property.serializedObject.ApplyModifiedProperties();
                    property.serializedObject.Update();
                    if (cachedEvents.Count == clipAsset.Events.Length)
                    {
                        for (var i = 0; i < clipAsset.Events.Length; i++)
                        {
                            var cachedEvent = cachedEvents[i];
                            var currentEvent = clipAsset.Events[i];
                            if (!Mathf.Approximately(cachedEvent.NormalizedTime, currentEvent.NormalizedTime))
                            {
                                eventMarkerDragIndex = i;
                                preview.NormalizedSampleTime = currentEvent.NormalizedTime;
                                break;
                            }
                        }
                    }
                }
            }
            
            HandleEvents();
        }

        private void DrawAddRemoveEventButton(Rect addEventButtonRect)
        {
            GUI.color = Color.white;
            if (eventMarkerDragIndex >= 0)
            {
                //remove button
                if (GUI.Button(addEventButtonRect, RemoveEventContent, AddRemoveEventStyle))
                {
                    RemoveEvent(eventMarkerDragIndex);
                }
            }
            else
            {
                //add button
                if (GUI.Button(addEventButtonRect, AddEventContent, AddRemoveEventStyle))
                {
                    AddEvent(timeMarkerTime);
                }
            }
        }

        private void AddEvent(float normalizedTime)
        {
            clipAsset.Events = clipAsset.Events.Append(new Authoring.AnimationClipEvent()
            {
                NormalizedTime = normalizedTime
            }).ToArray();
            ClearSelection();
            eventMarkerDragIndex = clipAsset.Events.Length - 1;
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }

        private void RemoveEvent(int index)
        {
            if (index >= 0 && index < clipAsset.Events.Length)
            {
                var l = clipAsset.Events.ToList();
                l.RemoveAt(index);
                clipAsset.Events = l.ToArray();
            }
            ClearSelection();
            property.serializedObject.ApplyModifiedProperties();
            property.serializedObject.Update();
        }
        
        private void ClearSelection()
        {
            isDraggingTimeMarker = false;
            eventMarkerDragIndex = -1;
        }

        private void HandleEvents()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    OnMouseDown(Event.current);
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(Event.current);
                    break;
            }
        }

        private void OnMouseDown(Event current)
        {
            ClearSelection();
            //reverse order for handling click ("last on top")
            for (var i = eventMarkers.Count - 1; i >= 0; i--)
            {
                if (eventMarkers[i].ControlRect.Contains(current.mousePosition))
                {
                    eventMarkerDragIndex = i;
                    current.Use();
                    break;
                }
            }

            isDraggingTimeMarker = true;

            if (eventMarkerDragIndex > -1) {
                var time = clipAsset.Events[eventMarkerDragIndex].NormalizedTime;
                timeMarkerTime = time;
                preview.NormalizedSampleTime = time;
                current.Use();
            }
            else {
                SetTimeMarkerPosition(current.mousePosition.x);
                current.Use();
            }

            if (current.clickCount == 2 && dragArea.Contains(current.mousePosition))
            {
                var time = PixelsToNormalizedTime(current.mousePosition.x, dragArea);
                AddEvent(time);
                current.Use();
            }
        }

        private void SetTimeMarkerPosition(float xPosition)
        {
            var time = PixelsToNormalizedTime(xPosition, dragArea);
            timeMarkerTime = time;
            preview.NormalizedSampleTime = time;
        }

        private void OnMouseDrag(in Event currentEvent)
        {
            if (isDraggingTimeMarker)
            {
                SetTimeMarkerPosition(currentEvent.mousePosition.x);
                currentEvent.Use();
            }

            if (eventMarkerDragIndex >= 0)
            {
                var eventMarker = eventMarkers[eventMarkerDragIndex];
                var normalizedTime = PixelsToNormalizedTime(currentEvent.mousePosition.x, dragArea);
                preview.NormalizedSampleTime = normalizedTime;

                clipAsset.Events[eventMarkerDragIndex].NormalizedTime = normalizedTime;

                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                
                currentEvent.Use();
            }
        }

        private void DrawEventsDragArea(Rect area)
        {
            {
                var dragAreaRect = area;
                var dragAreaColor = Color.black;
                dragAreaColor.a = 0.2f;
                GUI.color = dragAreaColor;
                GUI.DrawTexture(dragAreaRect, WhiteTex);
            }
            {
                var bottomBarRect = area;
                var barHeight = bottomBarRect.height * 0.1f;
                bottomBarRect.y = bottomBarRect.y + bottomBarRect.height - barHeight;
                bottomBarRect.height = barHeight;
                GUI.color = Color.gray;
                GUI.DrawTexture(bottomBarRect, WhiteTex);
            }
            {
                timeMarker = new RectElement(area.position, area.height, 10, 2);
                timeMarker.Position.x = NormalizedTimeToPixels(timeMarkerTime, area);
                GUI.color = Color.red;
                GUI.DrawTexture(timeMarker.VisualRect, WhiteTex);
            }
            {
                eventMarkers.Clear();
                for (var i = 0; i < clipAsset.Events.Length; i++)
                {
                    var e = clipAsset.Events[i];
                    var eventMarkerRect = new RectElement(area.position, area.height, 30, 5);
                    eventMarkerRect.Height *= 0.9f;
                    eventMarkerRect.Position.x = NormalizedTimeToPixels(e.NormalizedTime, area);

                    GUI.color = i == eventMarkerDragIndex ? selectedEventColor : unselectedEventColor;
                    GUI.DrawTexture(eventMarkerRect.VisualRect, EventMarkerTex, ScaleMode.ScaleAndCrop);
                    eventMarkers.Add(eventMarkerRect);
                }
            }
        }

        private float NormalizedTimeToPixels(float time, in Rect parentRect)
        {
            var normalizedTime = Mathf.Clamp01(time);
            return parentRect.xMin + parentRect.width * normalizedTime;
        }

        private float PixelsToNormalizedTime(float x, in Rect parentRect)
        {
            return Mathf.Clamp01((x - parentRect.xMin) / parentRect.width);
        }
    }
}