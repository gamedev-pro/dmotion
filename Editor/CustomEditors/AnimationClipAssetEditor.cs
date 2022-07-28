using System.Runtime.CompilerServices;
using DMotion.Authoring;
using UnityEditor;
using UnityEngine;

namespace DMotion.Editor
{
    [CustomEditor(typeof(AnimationClipAsset))]
    internal class AnimationClipAssetEditor : UnityEditor.Editor
    {
        private SingleClipPreview preview;
        private AnimationClipAsset ClipTarget => (AnimationClipAsset)target;
        
        private SerializedProperty clipProperty;
        private AnimationEventsPropertyDrawer eventsPropertyDrawer;
        
        private void OnEnable()
        {
            preview = new SingleClipPreview(ClipTarget.Clip);
            preview.Initialize();
            clipProperty = serializedObject.FindProperty(nameof(AnimationClipAsset.Clip));
            eventsPropertyDrawer = new AnimationEventsPropertyDrawer(
                ClipTarget,
                serializedObject.FindProperty(nameof(AnimationClipAsset.Events)),
                preview);
        }
        
        private void OnDisable()
        {
            preview?.Dispose();
        }

        public override void OnInspectorGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Preview Object");
                preview.GameObject = (GameObject)EditorGUILayout.ObjectField(preview.GameObject, typeof(GameObject), true);
            }
            using (var c = new EditorGUI.ChangeCheckScope())
            {
                EditorGUILayout.PropertyField(clipProperty, true);
                preview.Clip = ClipTarget.Clip;

                if (c.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                    serializedObject.Update();
                }
                
                var drawerRect = EditorGUILayout.GetControlRect();
                //TODO: This magic number is a right padding. Not why this is needed or of a better alternative
                drawerRect.xMax -= 60;
                eventsPropertyDrawer.OnInspectorGUI(drawerRect);
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            if (AnimationMode.InAnimationMode())
            {
                preview?.DrawPreview(r, background);
            }
        }
    }
}