using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public abstract class AnimationParameterAsset : ScriptableObject
    {
        public string Name;
        public int Hash => Name.GetHashCode();
    }
}