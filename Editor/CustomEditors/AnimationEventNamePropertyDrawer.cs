using System;
using System.Linq;
using DOTSAnimation.Authoring;
using UnityEditor;
using UnityEngine;

namespace DOTSAnimation.Editor
{
    [CustomPropertyDrawer(typeof(AnimationEventName))]
    public class AnimationEventNamePropertyDrawer : PropertyDrawer
    {
        private AnimationEventName[] allEventNameAssets;
        private string[] allEventNameOptions;
        private AnimationEventName[] EventNameAssets
        {
            get
            {
                if (allEventNameAssets == null)
                {
                    AssetDatabase.Refresh();
                    var guids = AssetDatabase.FindAssets($"t:{nameof(AnimationEventName)}");
                    allEventNameAssets = guids.Select(g =>
                        AssetDatabase.LoadAssetAtPath<AnimationEventName>(AssetDatabase.GUIDToAssetPath(g)))
                        .ToArray();
                }

                return allEventNameAssets;
            }
        }

        private string[] EventNameOptions
        {
            get
            {
                if (allEventNameOptions == null)
                {
                    allEventNameOptions = EventNameAssets.Select(e => e.name).ToArray();
                }
                return allEventNameOptions;
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var currEventName = property.objectReferenceValue as AnimationEventName;
            var index = Array.FindIndex(EventNameAssets, e => e == currEventName);
            var newIndex = EditorGUI.Popup(position, index, EventNameOptions);
            if (index != newIndex)
            {
                property.objectReferenceValue = EventNameAssets[newIndex];
            }
        }
    }
}