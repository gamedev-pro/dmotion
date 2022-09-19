using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public abstract class AnimationParameterAsset : StateMachineSubAsset
    {
        public int Hash => ((FixedString64Bytes) name).GetHashCode();
    }
}