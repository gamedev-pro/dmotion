using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace DOTSAnimation.Editor
{
    [CustomPropertyDrawer(typeof(AnimationClipEvent))]
    public class AnimationEventsPropertyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var inspector = new VisualElement();
            inspector.Add(new PropertyField(property.FindPropertyRelative(nameof(AnimationClipEvent.Name))));
            inspector.Add(new PropertyField(property.FindPropertyRelative(nameof(AnimationClipEvent.NormalizedTime))));
            return inspector;
        }
    }
}