using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public abstract class AnimationStateAsset : ScriptableObject
    {
        public bool Loop = true;
        public float Speed = 1;
    }
}