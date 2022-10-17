using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;

namespace DMotion.Editor
{
    public static class EditorSerializationUtils
    {
        private const BindingFlags AllBindingFlags = (BindingFlags)(-1);

        public static TAttribute GetAttribute<TAttribute>(this SerializedProperty serializedProperty,
            bool inherit = true)
            where TAttribute : Attribute
        {
            if (serializedProperty == null)
            {
                throw new ArgumentNullException(nameof(serializedProperty));
            }

            var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

            if (targetObjectType == null)
            {
                throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
            }

            foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
            {
                var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
                if (fieldInfo != null)
                {
                    return fieldInfo.GetCustomAttribute<TAttribute>(inherit);
                }

                var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
                if (propertyInfo != null)
                {
                    return propertyInfo.GetCustomAttribute<TAttribute>(inherit);
                }
            }

            throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
        }
        public static IEnumerable<TAttribute> GetAttributes<TAttribute>(this SerializedProperty serializedProperty, bool inherit = true)
            where TAttribute : Attribute
        {
            if (serializedProperty == null)
            {
                throw new ArgumentNullException(nameof(serializedProperty));
            }

            var targetObjectType = serializedProperty.serializedObject.targetObject.GetType();

            if (targetObjectType == null)
            {
                throw new ArgumentException($"Could not find the {nameof(targetObjectType)} of {nameof(serializedProperty)}");
            }

            foreach (var pathSegment in serializedProperty.propertyPath.Split('.'))
            {
                var fieldInfo = targetObjectType.GetField(pathSegment, AllBindingFlags);
                if (fieldInfo != null)
                {
                    return fieldInfo.GetCustomAttributes<TAttribute>(inherit);
                }

                var propertyInfo = targetObjectType.GetProperty(pathSegment, AllBindingFlags);
                if (propertyInfo != null)
                {
                    return propertyInfo.GetCustomAttributes<TAttribute>(inherit);
                }
            }

            throw new ArgumentException($"Could not find the field or property of {nameof(serializedProperty)}");
        }
    }
}