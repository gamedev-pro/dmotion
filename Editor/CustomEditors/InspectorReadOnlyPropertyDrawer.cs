using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEngine;

namespace DOTSAnimation.Editor
{
    [CustomPropertyDrawer(typeof(InspectorReadOnly))]
    public class InspectorReadOnlyPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(property, label);
            }
        }
    }
}