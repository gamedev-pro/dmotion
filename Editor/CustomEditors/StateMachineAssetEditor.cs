using DMotion.Authoring;
using UnityEditor;

namespace DMotion.Editor
{
    [CustomEditor(typeof(StateMachineAsset))]
    internal class StateMachineAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Double click the State Machine asset to open the visual editor", MessageType.Info);
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }
    }
}