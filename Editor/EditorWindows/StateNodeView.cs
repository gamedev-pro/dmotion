using System;
using System.Collections.Generic;
using System.Reflection;
using DMotion.Authoring;
using Unity.Entities;
using Unity.Entities.Editor;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DMotion.Editor
{
    internal class LinearBlendStateNodeView : StateNodeView<LinearBlendStateAsset>
    {
        public LinearBlendStateNodeView(VisualTreeAsset asset) : base(asset)
        {
        }
    }

    internal class SingleClipStateNodeView : StateNodeView<SingleClipStateAsset>
    {
        public SingleClipStateNodeView(VisualTreeAsset asset) : base(asset)
        {
        }
    }

    internal class StateNodeView<T> : StateNodeView
        where T : AnimationStateAsset
    {
        public StateNodeView(VisualTreeAsset asset) : base(asset)
        {
        }
    }

    internal struct StateNodeViewModel
    {
        internal AnimationStateMachineEditorView ParentView;
        internal AnimationStateAsset StateAsset;
        internal EntitySelectionProxyWrapper SelectedEntity;

        internal StateNodeViewModel(AnimationStateMachineEditorView parentView,
            AnimationStateAsset stateAsset,
            EntitySelectionProxyWrapper selectedEntity)
        {
            ParentView = parentView;
            StateAsset = stateAsset;
            SelectedEntity = selectedEntity;
        }
    }

    internal struct AnimationStateStyle
    {
        internal string ClassName;
        internal static AnimationStateStyle Default => new() { ClassName = "defaultstate" };
        internal static AnimationStateStyle Normal => new() { ClassName = "normalstate" };
        internal static AnimationStateStyle Active => new() { ClassName = "activestate" };


        internal static IEnumerable<AnimationStateStyle> AllStyles
        {
            get
            {
                yield return Default;
                yield return Normal;
                yield return Active;
            }
        }

        public override int GetHashCode()
        {
            return ClassName.GetHashCode();
        }

        public static bool operator ==(AnimationStateStyle left, AnimationStateStyle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AnimationStateStyle left, AnimationStateStyle right)
        {
            return !left.Equals(right);
        }
    }

    internal abstract class StateNodeView : Node
    {
        protected StateNodeViewModel model;

        internal Action<StateNodeView> StateSelectedEvent;
        public AnimationStateAsset State => model.StateAsset;
        public EntitySelectionProxyWrapper SelectedEntity => model.SelectedEntity;
        public StateMachineAsset StateMachine => model.ParentView.StateMachine;
        public Port input;
        public Port output;
        private ProgressBar timelineBar;

        protected StateNodeView(VisualTreeAsset asset) : base(AssetDatabase.GetAssetPath(asset))
        {
        }

        public static StateNodeView New(in StateNodeViewModel model)
        {
            StateNodeView view = model.StateAsset switch
            {
                SingleClipStateAsset _ => new SingleClipStateNodeView(model.ParentView.StateNodeXml),
                LinearBlendStateAsset _ => new LinearBlendStateNodeView(model.ParentView.StateNodeXml),
                _ => throw new NotImplementedException()
            };

            view.model = model;
            view.title = view.State.name;
            view.viewDataKey = view.State.StateEditorData.Guid;
            view.timelineBar = view.Q<ProgressBar>();

            view.SetPosition(new Rect(view.State.StateEditorData.GraphPosition, Vector2.one));

            view.CreateInputPort();
            view.CreateOutputPort();

            view.SetNodeStateStyle(view.GetStateStyle());


            return view;
        }

        #if UNITY_EDITOR || DEBUG
        internal void UpdateView()
        {
            var style = GetStateStyle();
            SetNodeStateStyle(style);

            if (style == AnimationStateStyle.Active)
            {
                UpdateTimelineProgressBar();
            }
        }
        #endif

        internal AnimationStateStyle GetStateStyle()
        {
            if (Application.isPlaying && SelectedEntity != null && SelectedEntity.Exists)
            {
                var stateMachine = SelectedEntity.GetComponent<AnimationStateMachine>();
                var currentAnimationState = SelectedEntity.GetComponent<AnimationCurrentState>();
                if (stateMachine.CurrentState.IsValid && stateMachine.CurrentState.AnimationStateId ==
                    currentAnimationState.AnimationStateId)
                {
                    var currentState = StateMachine.States[stateMachine.CurrentState.StateIndex];
                    if (currentState == State)
                    {
                        return AnimationStateStyle.Active;
                    }
                }
            }

            if (StateMachine.IsDefaultState(State))
            {
                return AnimationStateStyle.Default;
            }

            return AnimationStateStyle.Normal;
        }

        internal void SetNodeStateStyle(in AnimationStateStyle stateStyle)
        {
            foreach (var s in AnimationStateStyle.AllStyles)
            {
                RemoveFromClassList(s.ClassName);
            }

            AddToClassList(stateStyle.ClassName);

            timelineBar.style.display =
                stateStyle == AnimationStateStyle.Active ? DisplayStyle.Flex : DisplayStyle.None;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            var status = Application.isPlaying
                ? DropdownMenuAction.Status.Disabled
                : DropdownMenuAction.Status.Normal;
            evt.menu.AppendAction($"Create Transition", OnContextMenuCreateTransition, status);

            var setDefaultStateMenuStatus = StateMachine.IsDefaultState(State) || Application.isPlaying
                ? DropdownMenuAction.Status.Disabled
                : DropdownMenuAction.Status.Normal;

            evt.menu.AppendAction($"Set As Default State", OnContextMenuSetAsDefaultState, setDefaultStateMenuStatus);

            evt.StopPropagation();
        }

        private void OnContextMenuSetAsDefaultState(DropdownMenuAction obj)
        {
            var previousState = model.ParentView.GetViewForState(StateMachine.DefaultState);
            if (previousState != null)
            {
                previousState.SetNodeStateStyle(AnimationStateStyle.Normal);
            }

            StateMachine.SetDefaultState(State);
            SetNodeStateStyle(AnimationStateStyle.Default);
        }

        private void OnContextMenuCreateTransition(DropdownMenuAction obj)
        {
            //TODO (hack): There should be a better way to create an edge
            var ev = MouseDownEvent.GetPooled(Input.mousePosition, 0, 1, Vector2.zero);
            output.edgeConnector.GetType().GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(output.edgeConnector, new object[] { ev });
        }

        protected void CreateInputPort()
        {
            input = Port.Create<TransitionEdge>(Orientation.Vertical, Direction.Input, Port.Capacity.Multi,
                typeof(bool));
            input.portName = "";
            inputContainer.Add(input);
        }

        protected void CreateOutputPort()
        {
            output = Port.Create<TransitionEdge>(Orientation.Vertical, Direction.Output, Port.Capacity.Multi,
                typeof(bool));
            output.portName = "";
            outputContainer.Add(output);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            State.StateEditorData.GraphPosition = new Vector2(newPos.xMin, newPos.yMin);
            EditorUtility.SetDirty(State);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            StateSelectedEvent?.Invoke(this);
        }

        private void UpdateTimelineProgressBar()
        {
            var stateMachine = SelectedEntity.GetComponent<AnimationStateMachine>();

            if (!stateMachine.CurrentState.IsValid)
            {
                return;
            }

            var animationStates = SelectedEntity.GetBuffer<AnimationState>();
            var samplers = SelectedEntity.GetBuffer<ClipSampler>();
            var animationState = animationStates.GetWithId((byte)stateMachine.CurrentState.AnimationStateId);

            var avgTime = 0.0f;
            var avgDuration = 0.0f;
            var startSamplerIndex = samplers.IdToIndex(animationState.StartSamplerId);
            for (var i = startSamplerIndex; i < startSamplerIndex + animationState.ClipCount; i++)
            {
                avgTime += samplers[i].Time;
                avgDuration += samplers[i].Duration;
            }

            avgTime /= animationState.ClipCount;
            avgDuration /= animationState.ClipCount;

            var percent = avgTime / avgDuration;
            timelineBar.value = percent;
        }
    }
}