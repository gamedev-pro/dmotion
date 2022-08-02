using UnityEngine;

namespace DMotion.Samples
{
    public class SamplesConvertToEntity : MonoBehaviour
    {
        private void Awake()
        {
            #if NETCODE_PROJECT
            gameObject.AddComponent<Unity.NetCode.ConvertToClientServerEntity>();
            #else
            gameObject.AddComponent<Unity.Entities.ConvertToEntity>();
            #endif
        }
    }
}