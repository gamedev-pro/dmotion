using System;
using DMotion.Authoring;
using Unity.Entities.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DMotion.Editor
{
    internal struct TransitionEdgeModel
    {
        internal StateMachineAsset StateMachineAsset;
        internal EntitySelectionProxy SelectedEntity;
        internal int TransitionCount;
    }

    public class TransitionEdge : Edge
    {
        internal TransitionEdgeModel Model;

        internal Action<TransitionEdge> TransitionSelectedEvent;
        internal AnimationStateAsset FromState => (output.node as StateNodeView)?.State;
        internal AnimationStateAsset ToState => (input.node as StateNodeView)?.State;

        public TransitionEdge()
        {
            AddToClassList("transition");
        }

        #if UNITY_EDITOR || DEBUG
        public void UpdateView()
        {
            const string activeTransitionClassName = "active-transition";
            RemoveFromClassList(activeTransitionClassName);
            if (Application.isPlaying && Model.SelectedEntity != null && Model.SelectedEntity.Exists)
            {
                var currentAnimationState = Model.SelectedEntity.GetComponent<AnimationCurrentState>();
                var currentTransition = Model.SelectedEntity.GetComponent<AnimationStateTransition>();
                var stateMachine = Model.SelectedEntity.GetComponent<AnimationStateMachine>();
                if (currentTransition.IsValid && stateMachine.PreviousState.IsValid &&
                    stateMachine.CurrentState.IsValid &&
                    currentAnimationState.AnimationStateId == stateMachine.PreviousState.AnimationStateId &&
                    currentTransition.AnimationStateId == stateMachine.CurrentState.AnimationStateId)
                {
                    var currentTransitionFromState =
                        Model.StateMachineAsset.States[stateMachine.PreviousState.StateIndex];
                    var currentTransitionToState = Model.StateMachineAsset.States[stateMachine.CurrentState.StateIndex];
                    if (currentTransitionFromState == FromState && currentTransitionToState == ToState)
                    {
                        AddToClassList(activeTransitionClassName);
                    }
                }
            }
        }
        #endif

        public override void OnSelected()
        {
            base.OnSelected();
            TransitionSelectedEvent?.Invoke(this);
        }

        protected override EdgeControl CreateEdgeControl()
        {
            return new TransitionEdgeControl
            {
                Edge = this,
                capRadius = 4f,
                interceptWidth = 6f
            };
        }
    }
}