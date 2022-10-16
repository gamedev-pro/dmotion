using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace DMotion.Editor
{
    public class SelectSerializableTypePopup : EditorWindow
    {
        private Type[] types = new Type[0];
        private GUIContent[] typeNames = new GUIContent[0];
        private string searchString = "";

        private static HashSet<Assembly> invalidAssemblies;

        private Vector2 scrollPos;

        private Type baseType;
        private Action<Type> onSelected;
        private Predicate<Type> filter;

        public void Show(Type currentType, Type baseType, Action<Type> onSelected, Predicate<Type> filter)
        {
            if (baseType == null)
            {
                baseType = typeof(object);
            }

            this.baseType = baseType;
            this.onSelected = onSelected;
            this.filter = filter;

            if (currentType != null)
            {
                searchString = currentType.Name;
            }

            UpdateTypes();

            base.Show();
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void RefreshInvalidAssemblies()
        {
            invalidAssemblies = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a =>
                    a.FullName.StartsWith("System.", true, CultureInfo.CurrentCulture) ||
                    a.FullName.StartsWith("Unity.", true, CultureInfo.CurrentCulture) ||
                    a.FullName.StartsWith("com.unity", true, CultureInfo.CurrentCulture) ||
                    a.FullName.StartsWith("Microsoft") ||
                    a.FullName.StartsWith("Mono")).ToHashSet();
        }

        private void OnGUI()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                DrawSearchField();
                DrawOptions();
            }
        }

        int selected = -1;

        private void DrawOptions()
        {
            using (var scrollView = new EditorGUILayout.ScrollViewScope(scrollPos))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    var prevSelected = selected;
                    selected = GUILayout.SelectionGrid(selected, typeNames, 1);
                    if (selected != prevSelected)
                    {
                        var selectedType = types[selected];
                        onSelected?.Invoke(selectedType);
                        Close();
                    }
                }

                scrollPos = scrollView.scrollPosition;
            }
        }

        private void DrawSearchField()
        {
            var prev = searchString;
            searchString = EditorGUILayout.TextField(searchString, EditorStyles.toolbarSearchField);
            if (prev != searchString)
            {
                UpdateTypes();
            }
        }

        private void UpdateTypes()
        {
            types = TypeCache.GetTypesDerivedFrom(baseType)
                .Where(t => IsValidType(t) && MatchesSearch(t))
                .OrderBy(t => t.FullName)
                .ToArray();

            typeNames = types.Select(t => new GUIContent(t.Name)).ToArray();
        }

        private bool IsValidType(Type t)
        {
            var passesFilter = filter == null || filter(t);
            if (passesFilter)
            {
                return !invalidAssemblies.Contains(t.Assembly);
            }
            return false;
        }

        private bool MatchesSearch(Type t)
        {
            long score = 0;
            return FuzzySearch.FuzzyMatch(searchString, t.FullName, ref score);
        }
    }
}