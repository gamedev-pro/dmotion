using UnityEngine;

namespace DMotion.ComparisonTest
{
    public class AnimatorComparisonUIEnabler : MonoBehaviour
    {
        [SerializeField] private GameObject obj;
        [SerializeField] private bool ShouldBeActive;

        private void Awake()
        {
            obj.SetActive(ShouldBeActive);
        }
    }
}