using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [CreateAssetMenu(menuName = "DOTSAnimation/State Machine")]
    public class AnimationStateMachineAsset : ScriptableObject
    {
        public List<SingleClipStateAsset> SingleClipStates;
        public List<AnimationParameterAsset> Parameters;
        public List<AnimationTransitionGroup> Transitions;

        public IEnumerable<AnimationClipAsset> Clips => SingleClipStates.Select(s => s.Clip);
    }
}