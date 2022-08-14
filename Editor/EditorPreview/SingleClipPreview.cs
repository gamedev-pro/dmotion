using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DMotion.Editor
{
    public class SingleClipPreview : PlayableGraphPreview
    {
        private float normalizedSampleTime;
        
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
        public override float SampleTime
        {
            get => NormalizedSampleTime * Clip.length;
        }
        public override float NormalizedSampleTime
        {
            get => normalizedSampleTime;
            set => normalizedSampleTime = Mathf.Clamp01(value);
        }
        
        public SingleClipPreview(AnimationClip clip)
        {
            Clip = clip;
            normalizedSampleTime = 0;
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