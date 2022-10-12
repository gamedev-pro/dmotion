using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    public static class EditorLayoutUtils
    {
        public static IEnumerable<Rect> HorizontalLayout(this Rect r, params float[] widths)
        {
            if (widths.Length == 0)
            {
                yield return r;
            }

            //normalize widths
            var sumWidths = widths.Sum();
            for (var i = 0; i < widths.Length; i++)
            {
                widths[i] /= sumWidths;
            }

            var spacing = EditorGUIUtility.standardVerticalSpacing;
            var current = r;
            current.width = r.width * widths[0];
            yield return current;
            for (var i = 1; i < widths.Length; i++)
            {
                var w = r.width* widths[i];
                var prevW = r.width * widths[i - 1];
                current.x += prevW + spacing;
                current.width = w - spacing;
                yield return current;
            }
        }
    }
}