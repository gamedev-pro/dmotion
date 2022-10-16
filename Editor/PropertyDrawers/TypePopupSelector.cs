using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    internal class EnumTypePopupSelector : TypePopupSelector
    {
        protected override bool TypeFilter(Type t)
        {
            return t != null && t.IsEnum;
        }
    }
    
    internal class TypePopupSelector
    {
        private Type filterType;
        protected virtual bool TypeFilter(Type t)
        {
            return filterType.IsAssignableFrom(t);
        }

        internal void DrawSelectionPopup(Rect position, GUIContent label, Type selected, Action<Type> onSelected)
        {
            if (label != GUIContent.none)
            {
                var prevXMax = position.xMax;
                position.width = EditorGUIUtility.labelWidth;
                
                EditorGUI.LabelField(position, label);
                position.xMin += EditorGUIUtility.labelWidth;
                
                position.xMax = prevXMax;
            }

            var selectedName = selected != null ? selected.Name : "NONE";
            if (EditorGUI.DropdownButton(position, new GUIContent(selectedName), FocusType.Passive))
            {
                var rect = position;
                rect.height = 400;
                var w = EditorWindow.GetWindowWithRect<SelectSerializableTypePopup>(rect, true, "Select Type", true);
                w.Show(selected, filterType, onSelected, TypeFilter);
            }
        }       
    }
}