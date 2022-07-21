using System;
using UnityEngine;

namespace DOTSAnimation.Authoring
{
    [Serializable]
    public enum RootMotionMode : byte
    {
        [Tooltip("No root motion")]
        Disabled,
        [Tooltip("Automatically apply root motion to the owner")]
        EnabledAutomatic,
        [Tooltip("Store root motion deltas, which can be applied by a separated system")]
        EnabledManual
    }
}