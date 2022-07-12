using UnityEngine.UIElements;

namespace DOTSAnimation.Editor
{
    public class AnimationEventsEditorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<AnimationEventsEditorView, UxmlTraits>{}

        private const string ButtonAddEvent = "button-add-event";
        private const string TimeDragger = "dragger-container";
        private const string DragArea = "unity-drag-container";

        public SliderDragger SampleTimeDragger;

        public void Initialize()
        {
            var timeDraggerElement = this.Q<VisualElement>(TimeDragger);
            var dragAreaElement = this.Q<VisualElement>(DragArea);
            SampleTimeDragger = new SliderDragger(timeDraggerElement, dragAreaElement);
        }
    }
}