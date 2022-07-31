using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace DMotion.Editor
{
    internal interface IStateMachineInspector<T>
        where T : struct
    {
        internal void SetModel(T model);
    }

    internal class StateMachineInspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<StateMachineInspectorView, UxmlTraits> { }

        private UnityEditor.Editor editor;
        private Vector2 scrollPos;

        public void SetInspector<TEditor, TModel>(Object obj, TModel model)
            where TEditor : UnityEditor.Editor, IStateMachineInspector<TModel>
            where TModel : struct
        {
            Object.DestroyImmediate(editor);
            
            Clear();
            editor = UnityEditor.Editor.CreateEditor(obj, typeof(TEditor));
            ((IStateMachineInspector<TModel>) editor).SetModel(model);
            
            var imgui = new IMGUIContainer(() =>
            {
                if (editor.target != null)
                {
                    using (var scope  = new EditorGUILayout.ScrollViewScope(scrollPos))
                    {
                        editor.OnInspectorGUI();
                        scrollPos = scope.scrollPosition;
                        editor.serializedObject.ApplyModifiedProperties();
                        editor.serializedObject.Update();
                        //
                    }
                }
            });
            Add(imgui);
        }
    
        public void ClearInspector()
        {
            if (editor != null)
                Object.DestroyImmediate(editor);

            Clear();
        }
    }
}