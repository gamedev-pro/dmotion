using System;
using System.Collections.Generic;
using UnityEngine;

namespace DMotion.Authoring
{
    public abstract class AnimationStateAsset : ScriptableObject
    {
        public bool Loop = true;
        public float Speed = 1;

        public abstract StateType Type { get; }
        public abstract int ClipCount { get; }
        public abstract IEnumerable<AnimationClipAsset> Clips { get; }

    #if UNITY_EDITOR
        [Serializable]
        internal struct EditorData
        {
            internal Vector2 Position;
            internal string Guid;
        }

        [SerializeField, HideInInspector]
        internal EditorData StateEditorData;
    #endif
    }
}