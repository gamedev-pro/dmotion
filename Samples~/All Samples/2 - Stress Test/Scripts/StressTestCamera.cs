using UnityEngine;

namespace DMotion.StressTest
{
    public class StressTestCamera : MonoBehaviour
    {
        [SerializeField]
        private float speed;

        private void LateUpdate()
        {
            if (!Mathf.Approximately(Input.mouseScrollDelta.y, 0))
            {
                transform.position += transform.forward * Input.mouseScrollDelta.y * speed;
            }
        }
    }
}