using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace DOTSAnimation.Editor
{
    public class SliderDragger : Clickable
    {
        private VisualElement dragArea;
        private VisualElement dragElement;
        public Action<float> ValueChangedEvent;
        private Vector2 startMousePos;
        public Vector2 delta => lastMousePosition - startMousePos;
        private float value;

        private float min => 0;
        private float max => 1;
        
        public float Value
        {
            get => value;
            set => SetValueClamped(value);
        }

        public SliderDragger(VisualElement dragElement, VisualElement dragArea) : base(OnClicked)
        {
            this.dragElement = dragElement;
            this.dragArea = dragArea;
            target = dragElement;
            target.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            SetPosition(NormalizedValueToPosition(value));
        }

        private static void OnClicked()
        {
        }

        public float PositionToNormalizedValue(float posX)
        {
            return posX / (dragArea.layout.width - dragElement.layout.width);
        }
        public float NormalizedValueToPosition(float n)
        {
            var halfWidth = dragElement.layout.width * 0.5f;
            return Mathf.Lerp(-halfWidth, dragArea.layout.width - halfWidth, n);
        }

        protected override void ProcessDownEvent(EventBase evt, Vector2 localPosition, int pointerId)
        {
            base.ProcessDownEvent(evt, localPosition, pointerId);
            startMousePos = localPosition;
        }

        protected override void ProcessMoveEvent(EventBase evt, Vector2 localPosition)
        {
            base.ProcessMoveEvent(evt, localPosition);
            if (evt is PointerMoveEvent or MouseMoveEvent)
            {
                var x = dragElement.transform.position.x + delta.x;
                Value = PositionToNormalizedValue(x);
            }
        }

        private void SetValueClamped(float newValue)
        {
            value = Mathf.Clamp(newValue, min, max);
            var x = NormalizedValueToPosition(value);
            SetPosition(x);
        }

        private void SetPosition(float x)
        {
            if (!float.IsNaN(x))
            {
                dragElement.transform.position = new Vector2(x, 0);
                ValueChangedEvent?.Invoke(value);
            }
        }
    }
}