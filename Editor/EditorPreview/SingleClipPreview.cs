using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace DMotion.Editor
{
    public class SingleClipPreview : PlayableGraphPreview
    {
        private float sampleTime;
        
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
            get => sampleTime;
            set => sampleTime = Mathf.Clamp(value, 0, Clip.length);
        }
        
        public SingleClipPreview(AnimationClip clip)
        {
            Clip = clip;
            sampleTime = 0;
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