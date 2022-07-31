using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DMotion.Editor
{
    public class SingleClipPreview : PlayableGraphPreview
    {
        private float sampleNormalizedTime;
        
        private AnimationClip clip;

        public AnimationClip Clip
        {
            get => clip;
            set
            {
                if (clip != value)
                {
                    clip = value;
                    RefreshPreviewObjects();
                }
            }
        }
        
        protected override IEnumerable<AnimationClip> Clips
        {
            get
            {
                yield return Clip;
            }
        }
        public float SampleNormalizedTime
        {
            get => sampleNormalizedTime;
            set => sampleNormalizedTime = Mathf.Clamp01(value);
        }
        protected override float SampleTime => SampleNormalizedTime * Clip.length;

        protected override float PercentageDone => SampleTime * 100f / Clip.length;
        
        protected override int FrameCount => Mathf.RoundToInt(Clip.frameRate * SampleTime);

        protected override string PreviewName => Clip.name;
        
        public SingleClipPreview(AnimationClip clip)
        {
            Clip = clip;
            SampleNormalizedTime = 0;
        }

        protected override PlayableGraph BuildGraph()
        {
            var graph = PlayableGraph.Create();
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var playableOutput = AnimationPlayableOutput.Create(graph, "Animation", animator);
            var clipPlayable = AnimationClipPlayable.Create(graph, Clip);
            playableOutput.SetSourcePlayable(clipPlayable);
            return graph;
        }
    }
}