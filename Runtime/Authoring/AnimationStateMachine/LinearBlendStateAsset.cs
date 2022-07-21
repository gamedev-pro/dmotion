using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [Serializable]
    public struct ClipWithThreshold
    {
        public AnimationClipAsset Clip;
        public float Threshold;
    }
    
    [CreateAssetMenu(menuName = "DOTSAnimation/States/Blend Tree 1D")]
    public class LinearBlendStateAsset : AnimationStateAsset
    {
        public ClipWithThreshold[] BlendClips;
        public FloatParameterAsset BlendParameter;
        
        public override StateType Type => StateType.LinearBlend;
        public override int ClipCount => BlendClips.Length;
        public override IEnumerable<AnimationClipAsset> Clips => BlendClips.Select(b => b.Clip);
    }
}