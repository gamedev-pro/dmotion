using UnityEngine;

namespace DMotion.Editor
{
    internal struct RectElement
    {
        public Rect Rect;
        public float ControlWidth;
        public float VisualWidth;

        public RectElement(Rect rect, float controlWidth, float visualWidth)
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
}