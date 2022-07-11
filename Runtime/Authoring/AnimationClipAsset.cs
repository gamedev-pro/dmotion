using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public struct AnimationClipEvent
    {
        public string Name;
        public float NormalizedTime;
    }
    
    [CreateAssetMenu(menuName = "Tools/DOTSAnimation/Clip")]
    public class AnimationClipAsset : ScriptableObject
    {
        public AnimationClip Clip;
        public AnimationClipEvent[] Events;
    }
}