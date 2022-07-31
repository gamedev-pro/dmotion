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
        public List<AnimationClipAsset> Clips = new List<AnimationClipAsset>();
        public int ClipCount => Clips.Count;

        public GameObject ClipPreviewGameObject;
    }
}