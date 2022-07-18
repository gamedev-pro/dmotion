using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [CreateAssetMenu(menuName = "DOTSAnimation/States/Single Clip")]
    public class SingleClipStateAsset : AnimationStateAsset
    {
        public AnimationClipAsset Clip;
    }
}