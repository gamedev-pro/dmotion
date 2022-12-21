using System;

namespace DMotion.Authoring
{
    [Serializable]
    public struct SingleClipRefConvertData
    {
        public AnimationClipAsset Clip;
        public float Speed;
        public static SingleClipRefConvertData Default => new () { Clip = null, Speed = 1 };
    }
}