using System.Collections.Generic;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public abstract class AnimationStateAsset : ScriptableObject
    {
        public bool Loop = true;
        public float Speed = 1;

        public abstract StateType Type { get; }
        public abstract int ClipCount { get; }
        public abstract IEnumerable<AnimationClipAsset> Clips { get; }
    }
}