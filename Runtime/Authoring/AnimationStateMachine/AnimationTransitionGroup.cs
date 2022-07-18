using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [Serializable]
    public class AnimationTransitionGroup
    {
        public AnimationStateAsset FromState;
        public AnimationStateAsset ToState;
        public float NormalizedTransitionDuration;
        public List<AnimationBoolTransition> BoolTransitions;
    }
}