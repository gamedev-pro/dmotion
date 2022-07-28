using System;
using System.Reflection;
using DMotion.Authoring;
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

        internal StateNodeViewModel(AnimationStateMachineEditorView parentView, AnimationStateAsset stateAsset)
        {
            ParentView = parentView;
            StateAsset = stateAsset;
        }
    }
    
    internal abstract class StateNodeView : Node
    {
        internal const string DefaultStateClassName = "defaultstate";
        internal const string NormalStateClassName = "normalstate";
        protected StateNodeViewModel model;
        
        internal Action<StateNodeView> StateSelectedEvent;
        public AnimationStateAsset State => model.StateAsset;
        public StateMachineAsset StateMachine => model.ParentView.StateMachine;
        public Port input;
        public Port output;

        protected StateNodeView(VisualTreeAsset asset) : base(AssetDatabase.GetAssetPath(asset))
        {
        }

        public static StateNodeView New(in StateNodeViewModel model)
        {
            StateNodeView view = model.StateAsset switch
            {
                SingleClipStateAsset => new SingleClipStateNodeView(model.ParentView.StateNodeXml),
                LinearBlendStateAsset => new LinearBlendStateNodeView(model.ParentView.StateNodeXml),
                _ => throw new NotImplementedException()
            };

            view.model = model;
            view.title = view.State.name;
            view.viewDataKey = view.State.StateEditorData.Guid;
            view.SetPosition(new Rect(view.State.StateEditorData.GraphPosition, Vector2.one));
            
            view.CreateInputPort();
            view.CreateOutputPort();

            view.SetDefaultState(view.StateMachine.IsDefaultState(view.State));
            
            return view;
        }
        
        internal void SetDefaultState(bool isDefault)
        {
            RemoveFromClassList(DefaultStateClassName);
            RemoveFromClassList(NormalStateClassName);
            if (isDefault)
            {
                AddToClassList(DefaultStateClassName);
            }
            else
            {
                AddToClassList(NormalStateClassName);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            evt.menu.AppendAction($"Create Transition", OnContextMenuCreateTransition);

            var setDefaultStateMenuStatus = StateMachine.IsDefaultState(State)
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
                previousState.SetDefaultState(false);
            }
            
            StateMachine.SetDefaultState(State);
            SetDefaultState(true);
        }

        private void OnContextMenuCreateTransition(DropdownMenuAction obj)
        {
            //TODO (hack): There should be a better way to create an edge
            var ev = MouseDownEvent.GetPooled(Input.mousePosition, 0, 1, Vector2.zero);
            output.edgeConnector.GetType().GetMethod("OnMouseDown", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(output.edgeConnector, new object[]{ev});
        }
        
        protected void CreateInputPort()
        {
            input = Port.Create<TransitionEdge>(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            input.portName = "";
            inputContainer.Add(input);
        }

        protected void CreateOutputPort()
        {
            output = Port.Create<TransitionEdge>(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            output.portName = "";
            outputContainer.Add(output);
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Undo.RecordObject(State, $"{name}: SetPosition");
            State.StateEditorData.GraphPosition = new Vector2(newPos.xMin, newPos.yMin);
            EditorUtility.SetDirty(State);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            StateSelectedEvent?.Invoke(this);
        }
    }
}