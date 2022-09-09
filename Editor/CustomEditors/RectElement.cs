using UnityEngine;

namespace DMotion.Editor
{
    internal struct RectElement
    {
        public Vector2 Position;
        public float Height;
        public float ControlWidth;
        public float VisualWidth;

        public RectElement(Vector2 position, float height, float controlWidth, float visualWidth)
        {
            Position = position;
            Height = height;
            ControlWidth = controlWidth;
            VisualWidth = visualWidth;
        }

        public Rect VisualRect
        {
            get
            {
                return new Rect(Position.x - VisualWidth/2, Position.y, VisualWidth, Height);
            }
        }
        public Rect ControlRect
        {
            get
            {
                return new Rect(Position.x - ControlWidth/2, Position.y, ControlWidth, Height);
            }
        }
    }
}