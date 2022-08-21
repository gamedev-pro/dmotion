using UnityEditor;
using UnityEngine;

namespace DMotion.Tests.Editor
{
    [CustomEditor(typeof(ECSTestsFixture), editorForChildClasses:true)]
    public class ECSTestsFixtureEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Debug.Log("HERE");
        }
    }
}