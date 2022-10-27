using UnityEngine;
using System;
using UnityEditor;

namespace DMotion.Editor
{
    public static class EditorGUIUtils
    {
        public static int GenericEnumPopup(Rect r, Type enumType, int current)
        {
            return GenericEnumPopup(r, enumType, current, GUIContent.none);

        }
        public static int GenericEnumPopup(Rect r, Type enumType, int current, GUIContent label)
        {
            if (enumType is { IsEnum: true })
            {
                var enumValue = (Enum) Enum.GetValues(enumType).GetValue(current);
                return (int) (object)EditorGUI.EnumPopup(r, label, enumValue);
            }
            else
            {
                EditorGUI.LabelField(r, "Invalid type");
                return -1;
            }
        }
        
        // ScriptAttributeUtility.GetFieldInfoFromProperty(property, out type);
        // if (type != (System.Type) null && type.IsEnum)
        // {
        //   EditorGUI.BeginChangeCheck();
        //   int num = EditorGUI.EnumNamesCache.IsEnumTypeUsingFlagsAttribute(type) ? EditorGUI.EnumFlagsField(position, label, property.intValue, type, false, EditorStyles.popup) : EditorGUI.EnumPopupInternal(position, label, property.intValue, type, (Func<Enum, bool>) null, false, EditorStyles.popup);
        //   if (!EditorGUI.EndChangeCheck())
        //     return;
        //   System.Type enumUnderlyingType = type.GetEnumUnderlyingType();
        //   if (num < 0 && (enumUnderlyingType == typeof (uint) || enumUnderlyingType == typeof (ushort) || enumUnderlyingType == typeof (byte)))
        //     property.longValue = (long) (uint) num;
        //   else
        //     property.intValue = num;
        // }
    }
}