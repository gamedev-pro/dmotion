using UnityEngine;

namespace DMotion.Authoring
{
    [CreateAssetMenu(menuName = StateMachineEditorConstants.DMotionPath + "/Event Name")]
    public class AnimationEventName : ScriptableObject
    {
        public int Hash => name.GetHashCode();
    }
}