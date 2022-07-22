using TMPro;
using Unity.Entities;
using UnityEngine.UI;

namespace DMotion.Samples.CompleteStateMachine
{
    [GenerateAuthoringComponent]
    public class StateMachineExampleUI : IComponentData
    {
        public Slider BlendSlider;
        public Button AtkButton;
        public Button AtkRmButton;
        public Toggle IsJumpingToggle;
        public Toggle IsFallingToggle;
    }
}