using System;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [Serializable]
    public struct AnimationClipEvent
    {
        public string Name;
        [Range(0,1)]
        public float NormalizedTime;
    }
    
    [CreateAssetMenu(menuName = "Tools/DOTSAnimation/Clip")]
    public class AnimationClipAsset : ScriptableObject
    {
        public AnimationClip Clip;
        public AnimationClipEvent[] Events;
    }
}