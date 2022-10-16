using System;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    [CustomPropertyDrawer(typeof(SerializableType))]
    internal class SerializableTypePropertyDrawer : PropertyDrawer
    {
        private TypePopupSelector typePopupSelector;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (typePopupSelector == null)
            {
                if (property.GetAttribute<EnumTypeFilterAttribute>() != null)
                {
                    typePopupSelector = new EnumTypePopupSelector();
                }
                else
                {
                    typePopupSelector = new TypePopupSelector();
                }
            }

            var typeNameProperty = property.FindPropertyRelative(nameof(SerializableType.AssemblyQualifiedTypeName));
            var currentType = string.IsNullOrEmpty(typeNameProperty.stringValue)
                ? null
                : Type.GetType(typeNameProperty.stringValue);
            typePopupSelector.DrawSelectionPopup(position, label, currentType,
                newType =>
                {
                    typeNameProperty.stringValue = newType != null ? newType.AssemblyQualifiedName : "";
                    typeNameProperty.serializedObject.ApplyModifiedProperties();
                });
        }
    }
}