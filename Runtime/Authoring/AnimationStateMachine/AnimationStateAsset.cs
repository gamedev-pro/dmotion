using System;
using System.Collections.Generic;
using UnityEngine;

namespace DMotion.Authoring
{
    public abstract class StateMachineSubAsset : ScriptableObject{}
    public abstract class AnimationStateAsset : StateMachineSubAsset
    {
        public bool Loop = true;
        public float Speed = 1;
        
        public List<StateOutTransition> OutTransitions = new List<StateOutTransition>();

        public abstract StateType Type { get; }
        public abstract int ClipCount { get; }
        public abstract IEnumerable<AnimationClipAsset> Clips { get; }

    #if UNITY_EDITOR
        [Serializable]
        internal struct EditorData
        {
            [SerializeField]
            internal Vector2 GraphPosition;
            
            [SerializeField]
            internal string Guid;
        }

        [SerializeField, HideInInspector]
        internal EditorData StateEditorData;
    #endif
    }
}