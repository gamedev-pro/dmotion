using Unity.Entities;

namespace DMotion.Samples.CompleteStateMachine
{
    public partial class StateMachineExampleUISystem : SystemBase
    {
        private static readonly int IsFallingHash = StateMachineParameterUtils.GetHashCode("IsFalling");
        private static readonly int SpeedHash = StateMachineParameterUtils.GetHashCode("Speed");
        private static readonly int MovementModeHash = StateMachineParameterUtils.GetHashCode("MovementMode");

        private bool playCircleSlash = false;
        private bool playSlash = false;

        protected override void OnCreate()
        {
            base.OnCreate();
            RequireSingletonForUpdate<StateMachineExampleOneShots>();
            RequireSingletonForUpdate<StateMachineExampleUI>();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            var ui = this.GetSingleton<StateMachineExampleUI>();
            ui.AtkButton.onClick.AddListener(OnAttackButtonPressed);
            ui.AtkRmButton.onClick.AddListener(OnAttackRmButtonPressed);
        }

        private void OnAttackRmButtonPressed()
        {
            playCircleSlash = true;
        }

        private void OnAttackButtonPressed()
        {
            playSlash = true;
        }

        protected override void OnUpdate()
        {
            var oneShots = GetSingleton<StateMachineExampleOneShots>();
            var stateMachineUI = this.GetSingleton<StateMachineExampleUI>();
            
            Entities
                .ForEach((
                    ref PlayOneShotRequest playOneShot,
                    ref DynamicBuffer<BlendParameter> blendParameters,
                    ref DynamicBuffer<BoolParameter> boolParameters,
                    ref DynamicBuffer<IntParameter> intParameters) =>
                {
                    boolParameters.SetParameter(IsFallingHash, stateMachineUI.IsFallingToggle.isOn);
                    blendParameters.SetParameter(SpeedHash, stateMachineUI.BlendSlider.value);
                    intParameters.SetParameter(MovementModeHash, stateMachineUI.MovementModeDropdown.value);

                    if (playSlash)
                    {
                        playOneShot = new PlayOneShotRequest(
                            oneShots.Clips,
                            oneShots.ClipEvents,
                            oneShots.SlashClipIndex);
                    }
                    else if (playCircleSlash)
                    {
                        playOneShot = new PlayOneShotRequest(
                            oneShots.Clips,
                            oneShots.ClipEvents,
                            oneShots.CircleSlashClipIndex);
                    }

                    playSlash = playCircleSlash = false;
                }).WithoutBurst().Run();
        }
    }
}