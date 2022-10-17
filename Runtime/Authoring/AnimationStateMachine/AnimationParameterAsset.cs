using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace DMotion.Authoring
{
    public abstract class AnimationParameterAsset : StateMachineSubAsset
    {
        public int Hash => StateMachineParameterUtils.GetHashCode(name);

        public abstract string ParameterTypeName { get; }
    }
}