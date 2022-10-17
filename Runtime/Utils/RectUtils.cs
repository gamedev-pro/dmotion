using UnityEngine;

namespace DMotion
{
    public static class RectUtils
    {
        public static Rect ShrinkRight(this Rect r, float delta)
        {
            r.width -= delta;
            return r;
        }

        public static Rect ShrinkLeft(this Rect r, float delta)
        {
            r.xMin += delta;
            return r;
        }
    }
}