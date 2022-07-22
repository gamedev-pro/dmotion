using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DMotion.Authoring
{
    [CreateAssetMenu(menuName = StateMachineEditorConstants.DMotionPath + "/State Machine")]
    public class StateMachineAsset : ScriptableObject
    {
        public AnimationStateAsset DefaultState;
        public List<SingleClipStateAsset> SingleClipStates;
        public List<LinearBlendStateAsset> LinearBlendStates;
        public List<BoolParameterAsset> BoolParameters;
        public List<FloatParameterAsset> FloatParameters;
        public List<AnimationTransitionGroup> Transitions;
        public IEnumerable<AnimationClipAsset> Clips => SingleClipStates
            .SelectMany(s => s.Clips)
            .Concat(LinearBlendStates.SelectMany(s => s.Clips));
        
        public int ClipCount => SingleClipStates.Sum(s => s.ClipCount) +
                                LinearBlendStates.Sum(s => s.ClipCount);
        public int StateCount => SingleClipStates.Count + LinearBlendStates.Count;

        public IEnumerable<AnimationStateAsset> States => SingleClipStates
            .Concat(LinearBlendStates.OfType<AnimationStateAsset>());
    }
}