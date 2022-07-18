using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [CreateAssetMenu(menuName = "DOTSAnimation/State Machine")]
    public class StateMachineAsset : ScriptableObject
    {
        public List<SingleClipStateAsset> SingleClipStates;
        public List<AnimationParameterAsset> Parameters;
        public List<AnimationTransitionGroup> Transitions;
        public IEnumerable<AnimationClipAsset> Clips => SingleClipStates.SelectMany(s => s.Clips);
        public int ClipCount => SingleClipStates.Sum(s => s.ClipCount);
        public int StateCount => SingleClipStates.Count;

        public IEnumerable<AnimationStateAsset> States => SingleClipStates;
    }
}