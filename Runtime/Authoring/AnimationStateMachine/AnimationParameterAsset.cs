using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    public abstract class AnimationParameterAsset : ScriptableObject
    {
        public string Name;
        public int Hash => Name.GetHashCode();
    }
}