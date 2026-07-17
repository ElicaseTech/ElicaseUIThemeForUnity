using UnityEditor;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public static class ElicaseThemeManager
    {
        public static void Apply(VisualElement root, ElicaseThemeContext context = null)
        {
            if (root == null)
            {
                return;
            }

            if (context == null)
            {
                context = ElicaseThemeContext.FromSettings();
            }

            ApplyStyleSheets(root, context);
            ApplyClasses(root, context);
        }

        public static void ApplyWindowContent(EditorWindow window, ElicaseThemeContext context = null)
        {
            if (window == null)
            {
                return;
            }

            var root = window.rootVisualElement;
            if (root == null)
            {
                return;
            }

            root.AddToClassList(ElicaseThemeTokens.ClassWindowContent);

            ElicaseEditorWindowKind kind;
            if (ElicaseEditorWindowThemeBridge.TryGetWindowKind(window, out kind))
            {
                ApplyWindowKindClass(root, kind);
            }
        }

        public static void RemoveWindowContent(EditorWindow window)
        {
            if (window == null)
            {
                return;
            }

            RemoveWindowContent(window.rootVisualElement);
        }

        public static void RemoveWindowContent(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            root.RemoveFromClassList(ElicaseThemeTokens.ClassWindowContent);
            root.RemoveFromClassList(ElicaseThemeTokens.ClassWindowHierarchy);
            root.RemoveFromClassList(ElicaseThemeTokens.ClassWindowInspector);
        }

        public static void Remove(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            foreach (var path in ElicaseThemeTokens.CommonStyleSheetPaths)
            {
                RemoveStyleSheet(root, path);
            }

            RemoveStyleSheet(root, ElicaseThemeTokens.GetVariantStyleSheetPath(ElicaseUiStyle.Material));
            RemoveStyleSheet(root, ElicaseThemeTokens.GetVariantStyleSheetPath(ElicaseUiStyle.UnityOriginal));

            root.RemoveFromClassList(ElicaseThemeTokens.ClassRoot);
            root.RemoveFromClassList(ElicaseThemeTokens.ClassMaterial);
            root.RemoveFromClassList(ElicaseThemeTokens.ClassUnityOriginal);
            root.RemoveFromClassList(ElicaseThemeTokens.ClassDark);
            root.RemoveFromClassList(ElicaseThemeTokens.ClassLight);
        }

        private static void ApplyStyleSheets(VisualElement root, ElicaseThemeContext context)
        {
            foreach (var path in ElicaseThemeTokens.CommonStyleSheetPaths)
            {
                AddStyleSheet(root, path);
            }

            RemoveStyleSheet(root, ElicaseThemeTokens.GetVariantStyleSheetPath(ElicaseUiStyle.Material));
            RemoveStyleSheet(root, ElicaseThemeTokens.GetVariantStyleSheetPath(ElicaseUiStyle.UnityOriginal));
            AddStyleSheet(root, ElicaseThemeTokens.GetVariantStyleSheetPath(context.Style));
        }

        private static void ApplyClasses(VisualElement root, ElicaseThemeContext context)
        {
            root.AddToClassList(ElicaseThemeTokens.ClassRoot);

            root.EnableInClassList(ElicaseThemeTokens.ClassMaterial, context.Style == ElicaseUiStyle.Material);
            root.EnableInClassList(ElicaseThemeTokens.ClassUnityOriginal, context.Style == ElicaseUiStyle.UnityOriginal);
            root.EnableInClassList(ElicaseThemeTokens.ClassDark, context.IsDark);
            root.EnableInClassList(ElicaseThemeTokens.ClassLight, !context.IsDark);
        }

        private static void ApplyWindowKindClass(VisualElement root, ElicaseEditorWindowKind kind)
        {
            root.EnableInClassList(ElicaseThemeTokens.ClassWindowHierarchy, kind == ElicaseEditorWindowKind.Hierarchy);
            root.EnableInClassList(ElicaseThemeTokens.ClassWindowInspector, kind == ElicaseEditorWindowKind.Inspector);
        }

        private static void AddStyleSheet(VisualElement root, string path)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (styleSheet == null || HasStyleSheet(root, styleSheet))
            {
                return;
            }

            root.styleSheets.Add(styleSheet);
        }

        private static void RemoveStyleSheet(VisualElement root, string path)
        {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(path);
            if (styleSheet != null)
            {
                root.styleSheets.Remove(styleSheet);
            }
        }

        private static bool HasStyleSheet(VisualElement root, StyleSheet styleSheet)
        {
            for (var index = 0; index < root.styleSheets.count; index++)
            {
                if (root.styleSheets[index] == styleSheet)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
