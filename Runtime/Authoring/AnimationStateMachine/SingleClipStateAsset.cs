using System.Collections.Generic;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [CreateAssetMenu(menuName = "DOTSAnimation/States/Single Clip")]
    public class SingleClipStateAsset : AnimationStateAsset
    {
        public AnimationClipAsset Clip;
        public override StateType Type => StateType.Single;
        public override int ClipCount => 1;
        public override IEnumerable<AnimationClipAsset> Clips
        {
            get { yield return Clip; }
        }
    }
}