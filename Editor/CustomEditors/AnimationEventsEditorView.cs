using System;
using System.Collections.Generic;
using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEngine;

namespace DOTSAnimation.Editor
{
    public struct DragRect
    {
        public Rect Rect;
        public float ControlWidth;
        public float VisualWidth;

        public DragRect(Rect rect, float controlWidth, float visualWidth)
        {
            Rect = rect;
            ControlWidth = controlWidth;
            VisualWidth = visualWidth;
        }
        
        public Rect VisualRect
        {
            get
            {
                Rect.width = VisualWidth;
                return Rect;
            }
        }
        public Rect ControlRect
        {
            get
            {
                Rect.width = ControlWidth;
                return Rect;
            }
        }
    }

    public class AnimationEventsPropertyDrawer
    {
        private static readonly Texture2D whiteTex;
        private static Texture2D eventMarkerTex;

        private Rect dragArea;
        private DragRect timeMarker;
        private float timeMarkerTime;
        private List<DragRect> eventMarkers = new();
        private bool isDraggingTimeMarker = false;
        private int eventMarkerDragIndex = -1;
        private readonly SingleClipPreview preview;
        private AnimationClipAsset clipAsset;

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

        public AnimationEventsPropertyDrawer(AnimationClipAsset clipAsset, SingleClipPreview preview)
        {
            this.clipAsset = clipAsset;
            this.preview = preview;
            timeMarkerTime = preview.SampleNormalizedTime;
        }


        public void OnInspectorGUI(
            Rect area,
            GUIContent label,
            SerializedProperty property)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.LabelField(area, label);

                dragArea = area;
                dragArea.x += 60;
                DrawEventsDragArea(dragArea);
            }
            GUI.color = Color.white;
            EditorGUILayout.PropertyField(property, GUIContent.none, true);
            
            HandleEvents();
        }

        private void HandleEvents()
        {
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    OnMouseDown(Event.current);
                    break;
                case EventType.MouseUp:
                    OnMouseUp();
                    break;
                case EventType.MouseMove:
                    break;
                case EventType.MouseDrag:
                    OnMouseDrag(Event.current);
                    break;
                case EventType.KeyDown:
                    break;
                case EventType.KeyUp:
                    break;
                case EventType.ScrollWheel:
                    break;
                case EventType.Repaint:
                    break;
                case EventType.Layout:
                    break;
                case EventType.DragUpdated:
                    break;
                case EventType.DragPerform:
                    break;
                case EventType.DragExited:
                    break;
                case EventType.Ignore:
                    break;
                case EventType.Used:
                    break;
                case EventType.ValidateCommand:
                    break;
                case EventType.ExecuteCommand:
                    break;
                case EventType.ContextClick:
                    break;
                case EventType.MouseEnterWindow:
                    break;
                case EventType.MouseLeaveWindow:
                    break;
                case EventType.TouchDown:
                    break;
                case EventType.TouchUp:
                    break;
                case EventType.TouchMove:
                    break;
                case EventType.TouchEnter:
                    break;
                case EventType.TouchLeave:
                    break;
                case EventType.TouchStationary:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnMouseUp()
        {
            isDraggingTimeMarker = false;
            eventMarkerDragIndex = -1;
        }

        private void OnMouseDown(Event current)
        {
            isDraggingTimeMarker = false;
            eventMarkerDragIndex = -1;
            for (var i = 0; i < eventMarkers.Count; i++)
            {
                if (eventMarkers[i].ControlRect.Contains(current.mousePosition))
                {
                    eventMarkerDragIndex = i;
                    current.Use();
                    return;
                }
            }
            
            if (timeMarker.ControlRect.Contains(current.mousePosition))
            {
                isDraggingTimeMarker = true;
                current.Use();
            }
        }

        private void OnMouseDrag(in Event currentEvent)
        {
            if (isDraggingTimeMarker)
            {
                var time = PixelsToNormalizedTime(currentEvent.mousePosition.x, timeMarker.VisualRect, dragArea);
                timeMarkerTime = time;
                preview.SampleNormalizedTime = time;
                currentEvent.Use();
            }
            else if (eventMarkerDragIndex >= 0)
            {
                var eventMarker = eventMarkers[eventMarkerDragIndex];
                var time = PixelsToNormalizedTime(currentEvent.mousePosition.x, eventMarker.VisualRect, dragArea);
                preview.SampleNormalizedTime = time;

                clipAsset.Events[eventMarkerDragIndex].NormalizedTime = time;
                EditorUtility.SetDirty(clipAsset);
                
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
                timeMarker = new DragRect(area, 10, 2);
                timeMarker.Rect.x = NormalizedTimeToPixels(timeMarkerTime, timeMarker.VisualRect, area);
                GUI.color = Color.red;
                GUI.DrawTexture(timeMarker.VisualRect, whiteTex);
            }
            {
                eventMarkers.Clear();
                for (var i = 0; i < clipAsset.Events.Length; i++)
                {
                    var e = clipAsset.Events[i];
                    var eventMarkerRect = new DragRect(area, 10, 5);
                    eventMarkerRect.Rect.height *= 0.9f;
                    eventMarkerRect.Rect.x = NormalizedTimeToPixels(e.NormalizedTime, eventMarkerRect.VisualRect, area);
                    GUI.color = Color.white;
                    GUI.DrawTexture(eventMarkerRect.VisualRect, EventMarkerTex, ScaleMode.ScaleAndCrop);
                    eventMarkers.Add(eventMarkerRect);
                }
            }
        }

        private float NormalizedTimeToPixels(float time, in Rect rect, in Rect parentRect)
        {
            return parentRect.xMin + (parentRect.width - rect.width)* time;
        }

        private float PixelsToNormalizedTime(float x, in Rect rect, in Rect parentRect)
        {
            return Mathf.Clamp01((x - parentRect.xMin) / (parentRect.width - rect.width));
        }
    }
}