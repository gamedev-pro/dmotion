using System.Linq;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    [CustomPropertyDrawer(typeof(AnimationEventName))]
    internal class AnimationEventNamePropertyDrawer : PropertyDrawer
    {
        private ObjectReferencePopupSelector<AnimationEventName> eventNamesSelector = new ObjectReferencePopupSelector<AnimationEventName>();
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            eventNamesSelector.OnGUI(position, property, label);
        }
    }
}