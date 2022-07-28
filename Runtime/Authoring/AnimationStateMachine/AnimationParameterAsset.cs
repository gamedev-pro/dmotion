using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public abstract class AnimationParameterAsset : ScriptableObject
    {
        public int Hash => name.GetHashCode();
    }
}