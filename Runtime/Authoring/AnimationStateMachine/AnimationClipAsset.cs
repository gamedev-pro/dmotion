using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace DMotion.Authoring
{
    [Serializable]
    public struct AnimationClipEvent
    {
        public AnimationEventName Name;
        [Min(0), FormerlySerializedAs("NormalizedTime")]
        public float Time;

        public int Hash => Name.Hash;
    }
    
    [CreateAssetMenu(menuName = StateMachineEditorConstants.DMotionPath + "/Clip")]
    public class AnimationClipAsset : ScriptableObject
    {
        public AnimationClip Clip;
        public AnimationClipEvent[] Events;
    }
}