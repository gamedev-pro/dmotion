using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEngine;

namespace DOTSAnimation.Editor
{
    [CustomEditor(typeof(AnimationClipAsset))]
    public class AnimationClipAssetEditor : UnityEditor.Editor
    {
        private SingleClipPreview preview;
        private AnimationClipAsset ClipTarget => (AnimationClipAsset)target;
        
        private SerializedProperty clipProperty;
        private SerializedProperty eventsProperty;
        private AnimationEventsPropertyDrawer eventsPropertyDrawer;
        
        private void OnEnable()
        {
            preview = new SingleClipPreview(ClipTarget.Clip);
            preview.Initialize();
            clipProperty = serializedObject.FindProperty(nameof(AnimationClipAsset.Clip));
            eventsProperty = serializedObject.FindProperty(nameof(AnimationClipAsset.Events));
            eventsPropertyDrawer = new AnimationEventsPropertyDrawer();
            
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
                
            var content = new GUIContent(eventsProperty.displayName);
            var drawerRect = EditorGUILayout.GetControlRect(true, eventsPropertyDrawer.GetPropertyHeight(eventsProperty, content));
            eventsPropertyDrawer.OnGUI(drawerRect, eventsProperty, content);
            
                if (c.changed)
                {
                    serializedObject.ApplyModifiedProperties();
                }
            }
        }

        public override bool HasPreviewGUI()
        {
            return true;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            preview.DrawPreview(r, background);
        }
    }
}