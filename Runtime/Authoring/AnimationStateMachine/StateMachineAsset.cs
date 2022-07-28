using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DMotion.Authoring
{
    [CreateAssetMenu(menuName = StateMachineEditorConstants.DMotionPath + "/State Machine")]
    public class StateMachineAsset : ScriptableObject
    {
        public AnimationStateAsset DefaultState;
        public List<AnimationStateAsset> States = new List<AnimationStateAsset>();
        public List<AnimationParameterAsset> Parameters = new List<AnimationParameterAsset>();

        public IEnumerable<AnimationClipAsset> Clips => States.SelectMany(s => s.Clips);
        public int ClipCount => States.Sum(s => s.ClipCount);
    }
}