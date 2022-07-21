using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [CreateAssetMenu(menuName = "Tools/DOTSAnimation/Event Name")]
    public class AnimationEventName : ScriptableObject
    {
        public int Hash => name.GetHashCode();
    }
}