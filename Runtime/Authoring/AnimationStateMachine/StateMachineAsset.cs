using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DMotion.Authoring
{
    [CreateAssetMenu(menuName = StateMachineEditorConstants.DMotionPath + "/State Machine")]
    public class StateMachineAsset : ScriptableObject
    {
        public AnimationStateAsset DefaultState;
        public List<SingleClipStateAsset> SingleClipStates = new List<SingleClipStateAsset>();
        public List<LinearBlendStateAsset> LinearBlendStates = new List<LinearBlendStateAsset>();
        public List<AnimationParameterAsset> Parameters = new List<AnimationParameterAsset>();
        public IEnumerable<AnimationClipAsset> Clips => SingleClipStates
            .SelectMany(s => s.Clips)
            .Concat(LinearBlendStates.SelectMany(s => s.Clips));
        
        public int ClipCount => SingleClipStates.Sum(s => s.ClipCount) +
                                LinearBlendStates.Sum(s => s.ClipCount);
        public int StateCount => SingleClipStates.Count + LinearBlendStates.Count;

        public IEnumerable<AnimationStateAsset> States => Enumerable.Empty<AnimationStateAsset>()
            .Concat(SingleClipStates)
            .Concat(LinearBlendStates);
    }
}