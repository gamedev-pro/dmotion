using UnityEngine;
using Random = UnityEngine.Random;

namespace DMotion.ComparisonTest
{
    //Note: this could be done in a system (and it would be faster), but the intention of this sample is to compare regular DOTS implementation with a regular Monobehaviour implementation
    public class UpdateAnimatorComponent : MonoBehaviour
    {
        private static readonly int SpeedHash = Animator.StringToHash("Speed");
        private static readonly int IsFallingHash = Animator.StringToHash("IsFalling");
        private static readonly int AttackTrigger = Animator.StringToHash("Attack");
        
        [SerializeField] private Animator animator;

        private float speed;
        private short linearBlendDirection = 1;
        private bool isFalling;

        private void Update()
        {
            var dt = Time.deltaTime;
            var integerPart = (uint)Time.time + 1;
            var decimalPart = (Time.time + 1) - integerPart;
            var shouldSwitchStates = decimalPart < dt && integerPart % 2 == 0;

            speed = Mathf.Clamp01(speed + linearBlendDirection * dt);
            animator.SetFloat(SpeedHash, speed);

            if (shouldSwitchStates)
            {
                var prob = Random.Range(0, 101);
                if (prob < 30)
                {
                    linearBlendDirection *= -1;
                }
                else if (prob < 60)
                {
                    isFalling = !isFalling;
                    animator.SetBool(IsFallingHash, isFalling);
                }
                else
                {
                    animator.SetTrigger(AttackTrigger);
                }
            }
        }
    }
}