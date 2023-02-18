using System;
using System.Linq;
using Latios.Authoring;
using Latios.Kinemation;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Assertions;

namespace DMotion.Authoring
{
    [Serializable]
    public struct SingleClipRefConvertData
    {
        public AnimationClipAsset Clip;
        public float Speed;
        public static SingleClipRefConvertData Default => new() { Clip = null, Speed = 1 };
    }
}