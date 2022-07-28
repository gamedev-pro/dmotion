using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DMotion.Editor
{
    internal class OnAssetsChangedPostProcessor : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            ObjectReferencePopupSelector.IsDirty = true;
        }
    }
    
    internal class SubAssetReferencePopupSelector<T> : ObjectReferencePopupSelector<T>
        where T : Object
    {
        private Object target;
        private Type filterType;

        internal SubAssetReferencePopupSelector(Object target, Type filterType = null)
        {
            this.target = target;
            this.filterType = filterType;
        }
        
        protected override T[] CollectOptions()
        {
            var childs = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(target))
                .OfType<T>();

            if (filterType != null)
            {
                childs = childs.Where(filterType.IsInstanceOfType);
            }

            return childs.ToArray();
        }
    }

    internal class ObjectReferencePopupSelector
    {
        internal static bool IsDirty;
    }
    internal class ObjectReferencePopupSelector<T> : ObjectReferencePopupSelector
        where T : Object
    {
        private T[] allEventNameAssets;
        private string[] allEventNameOptions;
        private T[] EventNameAssets
        {
            get
            {
                if (allEventNameAssets == null || IsDirty)
                {
                    allEventNameAssets = CollectOptions();
                }
 
                return allEventNameAssets;
            }
        }
 
        private string[] EventNameOptions
        {
            get
            {
                if (allEventNameOptions == null || IsDirty)
                {
                    allEventNameOptions = EventNameAssets.Select(e => e.name).ToArray();
                }
                return allEventNameOptions;
            }
        }
        
        protected virtual T[] CollectOptions()
        {
            AssetDatabase.Refresh();
            var guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            return guids.Select(g =>
                    AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .ToArray();
        }

        internal void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (label != GUIContent.none)
            {
                var prevXMax = position.xMax;
                position.width = EditorGUIUtility.labelWidth;
                
                EditorGUI.LabelField(position, label);
                position.xMin += EditorGUIUtility.labelWidth;
                
                position.xMax = prevXMax;
            }
             
            var currEventName = property.objectReferenceValue as T;
            var index = Array.FindIndex(EventNameAssets, e => e == currEventName);
            var newIndex = EditorGUI.Popup(position, index, EventNameOptions);
            if (index != newIndex)
            {
                property.objectReferenceValue = EventNameAssets[newIndex];
            }

            IsDirty = false;
        }       
    }
}