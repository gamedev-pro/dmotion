using System;
using System.Collections.Generic;
using System.Linq;
using DMotion.Authoring;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DMotion.Editor
{
    internal struct TransitionPair
    {
        internal AnimationStateAsset FromState;
        internal AnimationStateAsset ToState;

        public override int GetHashCode()
        {
            return FromState.GetHashCode() * ToState.GetHashCode();
        }

        internal TransitionPair(AnimationStateAsset from, AnimationStateAsset to)
        {
            FromState = from;
            ToState = to;
        }

        internal TransitionPair(AnimationStateAsset state, int outTransitionIndex)
        {
            var outTransition = state.OutTransitions[outTransitionIndex];
            FromState = state;
            ToState = outTransition.ToState;
        }
    }

    public class AnimationStateMachineEditorView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<AnimationStateMachineEditorView, UxmlTraits>
        {
        }

        private StateMachineEditorViewModel model;

        private Dictionary<AnimationStateAsset, StateNodeView> stateToView =
            new Dictionary<AnimationStateAsset, StateNodeView>();

        private Dictionary<TransitionPair, TransitionEdge> transitionToEdgeView =
            new Dictionary<TransitionPair, TransitionEdge>();

        internal StateMachineAsset StateMachine => model.StateMachineAsset;
        internal VisualTreeAsset StateNodeXml => model.StateNodeXml;

        public AnimationStateMachineEditorView()
        {
            var gridBg = new GridBackground();
            gridBg.name = "sm-grid-bg";
            Insert(0, gridBg);
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (model.StateMachineAsset != null)
            {
                var status = Application.isPlaying
                    ? DropdownMenuAction.Status.Disabled
                    : DropdownMenuAction.Status.Normal;
                evt.menu.AppendAction("New State", a => CreateState(a, typeof(SingleClipStateAsset)), status);
                evt.menu.AppendAction("New Blend Tree 1D", a => CreateState(a, typeof(LinearBlendStateAsset)), status);
            }

            evt.StopPropagation();
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphviewchange)
        {
            if (Application.isPlaying)
            {
                return graphviewchange;
            }

            if (graphviewchange.elementsToRemove != null)
            {
                foreach (var el in graphviewchange.elementsToRemove)
                {
                    if (el is StateNodeView stateView)
                    {
                        DeleteState(stateView.State);
                    }
                    else if (el is TransitionEdge transition)
                    {
                        if (transition.output.node is StateNodeView from &&
                            transition.input.node is StateNodeView to)
                        {
                            DeleteAllOutTransitions(from.State, to.State);
                        }
                    }
                }
            }

            if (graphviewchange.edgesToCreate != null)
            {
                foreach (var edge in graphviewchange.edgesToCreate)
                {
                    if (edge is TransitionEdge)
                    {
                        if (edge.output.node is StateNodeView fromStateView &&
                            edge.input.node is StateNodeView toStateView)
                        {
                            CreateOutTransition(fromStateView.State, toStateView.State);
                        }
                    }
                }

                graphviewchange.edgesToCreate.Clear();
            }

            return graphviewchange;
        }

        private void DeleteState(AnimationStateAsset state)
        {
            model.StateMachineAsset.DeleteState(state);
        }

        private void DeleteAllOutTransitions(AnimationStateAsset fromState, AnimationStateAsset toState)
        {
            fromState.OutTransitions.RemoveAll(t => t.ToState == toState);
            transitionToEdgeView.Remove(new TransitionPair(fromState, toState));
        }

        private void CreateState(DropdownMenuAction action, Type stateType)
        {
            var state = model.StateMachineAsset.CreateState(stateType);
            state.StateEditorData.GraphPosition = contentViewContainer.WorldToLocal(action.eventInfo.mousePosition);
            InstantiateStateView(state);
        }

        private void CreateOutTransition(AnimationStateAsset fromState, AnimationStateAsset toState)
        {
            var outTransition = new StateOutTransition(toState);
            fromState.OutTransitions.Add(outTransition);
            InstantiateTransitionEdge(fromState, fromState.OutTransitions.Count - 1);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList()
                .Where((nap => nap.direction != startPort.direction &&
                               nap.node != startPort.node)).ToList();
        }

        internal void UpdateDebug()
        {
            foreach (var stateView in stateToView.Values)
            {
                stateView.UpdateDebug();
            }
        }

        internal void PopulateView(in StateMachineEditorViewModel newModel)
        {
            model = newModel;
            stateToView.Clear();
            transitionToEdgeView.Clear();

            graphViewChanged -= OnGraphViewChanged;
            DeleteElements(graphElements.ToList());
            graphViewChanged += OnGraphViewChanged;

            foreach (var s in model.StateMachineAsset.States)
            {
                InstantiateStateView(s);
            }

            foreach (var t in model.StateMachineAsset.States)
            {
                for (var i = 0; i < t.OutTransitions.Count; i++)
                {
                    InstantiateTransitionEdge(t, i);
                }
            }

            model.ParametersInspectorView.SetInspector<ParametersInspector, ParameterInspectorModel>(
                model.StateMachineAsset, new ParameterInspectorModel()
                {
                    StateMachine = model.StateMachineAsset
                });
        }

        internal StateNodeView GetViewForState(AnimationStateAsset state)
        {
            return state == null ? null : stateToView.TryGetValue(state, out var view) ? view : null;
        }

        private void InstantiateTransitionEdge(AnimationStateAsset state, int outTransitionIndex)
        {
            var transitionPair = new TransitionPair(state, outTransitionIndex);
            if (transitionToEdgeView.TryGetValue(transitionPair, out var existingEdge))
            {
                existingEdge.TransitionCount++;
                existingEdge.MarkDirtyRepaint();
            }
            else
            {
                var fromStateView = GetViewForState(transitionPair.FromState);
                var toStateView = GetViewForState(transitionPair.ToState);
                var edge = fromStateView.output.ConnectTo<TransitionEdge>(toStateView.input);
                edge.TransitionCount = 1;
                AddElement(edge);
                transitionToEdgeView.Add(transitionPair, edge);

                edge.TransitionSelectedEvent += OnTransitionSelected;
            }
        }

        private void InstantiateStateView(AnimationStateAsset state)
        {
            var stateView = StateNodeView.New(new StateNodeViewModel(this, state, model.SelectedEntity));
            AddElement(stateView);
            stateToView.Add(state, stateView);

            stateView.StateSelectedEvent += OnStateSelected;
        }

        private void OnStateSelected(StateNodeView obj)
        {
            var inspectorModel = new AnimationStateInspectorModel
            {
                StateView = obj
            };
            switch (obj)
            {
                case SingleClipStateNodeView _:
                    model.InspectorView.SetInspector<SingleStateInspector, AnimationStateInspectorModel>
                        (inspectorModel.StateAsset, inspectorModel);
                    break;
                case LinearBlendStateNodeView _:
                    model.InspectorView.SetInspector<LinearBlendStateInspector, AnimationStateInspectorModel>
                        (inspectorModel.StateAsset, inspectorModel);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(obj));
            }
        }

        private void OnTransitionSelected(TransitionEdge obj)
        {
            var inspectorModel = new TransitionGroupInspectorModel()
            {
                FromState = obj.FromState,
                ToState = obj.ToState
            };
            model.InspectorView.SetInspector<TransitionGroupInspector, TransitionGroupInspectorModel>(
                inspectorModel.FromState, inspectorModel);
        }
    }
}