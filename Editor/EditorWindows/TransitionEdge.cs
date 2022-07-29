using System;
using DMotion.Authoring;
using UnityEditor.Experimental.GraphView;

namespace DMotion.Editor
{
    public class TransitionEdge : Edge
    {
        internal int TransitionCount;
        internal Action<TransitionEdge> TransitionSelectedEvent;
        internal AnimationStateAsset FromState => (output.node as StateNodeView)?.State;
        internal AnimationStateAsset ToState => (input.node as StateNodeView)?.State;
        public TransitionEdge() : base()
        {
        }

        public override void OnSelected()
        {
            base.OnSelected();
            TransitionSelectedEvent?.Invoke(this);
        }

        protected override EdgeControl CreateEdgeControl()
        {
            return new TransitionEdgeControl()
            {
                Edge = this,
                capRadius = 4f,
                interceptWidth = 6f
            };
        }
    }
}