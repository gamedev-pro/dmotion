using DOTSAnimation;
using Unity.Entities;

public partial class StateMachineExampleUISystem : SystemBase
{
    private static int IsJumpingHash => AnimationStateMachineUtils.GetHashCode("IsJumping");
    private static int IsFallingHash => AnimationStateMachineUtils.GetHashCode("IsFalling");
    private static int SpeedHash => AnimationStateMachineUtils.GetHashCode("Speed");
    
    private bool playAtkAnim = false;
    private bool playBlockAnim = false;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        var ui = this.GetSingleton<StateMachineExampleUI>();
        ui.AtkButton.onClick.AddListener(OnAttackButtonPressed);
        ui.AtkRmButton.onClick.AddListener(OnAttackRmButtonPressed);
    }
    private void OnAttackRmButtonPressed()
    {
        playAtkAnim = true;
    }

    private void OnAttackButtonPressed()
    {
        playBlockAnim = true;
    }

    protected override void OnUpdate()
    {
        var stateMachineUI = this.GetSingleton<StateMachineExampleUI>();
        Entities
            .WithChangeFilter<AnimationStateMachine>()
            .ForEach((ref AnimationStateMachine stateMachine,
                ref DynamicBuffer<BlendParameter> blendParameters,
                ref DynamicBuffer<BoolParameter> boolParameters,
                in DynamicBuffer<AnimationState> states,
                in CombatAnimations combatAnimations) =>
            {
                boolParameters.SetParameter(IsFallingHash, stateMachineUI.IsFallingToggle.isOn);
                boolParameters.SetParameter(IsJumpingHash, stateMachineUI.IsJumpingToggle.isOn);
                blendParameters.SetParameter(SpeedHash, stateMachineUI.BlendSlider.value);
                
                if (playAtkAnim)
                {
                    combatAnimations.Attack.PlayOneShot(ref stateMachine);
                }
                else if (playBlockAnim)
                {
                    combatAnimations.Block.PlayOneShot(ref stateMachine);
                }
            
                playAtkAnim = false;
                playBlockAnim = false;
            }).WithoutBurst().Run();
    }
}