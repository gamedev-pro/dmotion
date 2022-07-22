using System;
using UnityEngine;

namespace DMotion.Authoring
{
    [Serializable]
    public struct AnimationClipEvent
    {
        public AnimationEventName Name;
        [Range(0,1)]
        public float NormalizedTime;

        public int Hash => Name.Hash;
    }
    
    [CreateAssetMenu(menuName = StateMachineEditorConstants.DMotionPath + "/Clip")]
    public class AnimationClipAsset : ScriptableObject
    {
        public AnimationClip Clip;
        public AnimationClipEvent[] Events;
    }
}