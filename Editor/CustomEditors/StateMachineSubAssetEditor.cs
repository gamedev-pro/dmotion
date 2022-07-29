using DMotion.Authoring;
using UnityEditor;

namespace DMotion.Editor
{
    [CustomEditor(typeof(StateMachineSubAsset), true)]
    internal class StateMachineSubAssetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("You should use the Visual Editor to edit this asset.", MessageType.Warning);
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }
    }
}