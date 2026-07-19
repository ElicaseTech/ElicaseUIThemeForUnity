using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    [InitializeOnLoad]
    public static class ElicaseInspectorHeaderRenderer
    {
        private const float TitleLeftInset = 44f;
        private const float TitleRightInset = 72f;
        private const float TitleVerticalInset = 6f;

        private static readonly Dictionary<TextElement, TrackedTitle> trackedTitles =
            new Dictionary<TextElement, TrackedTitle>();
        private static readonly Dictionary<IMGUIContainer, TrackedImGuiHeader> trackedImGuiHeaders =
            new Dictionary<IMGUIContainer, TrackedImGuiHeader>();

        static ElicaseInspectorHeaderRenderer()
        {
        }

        public static void RequestRefresh()
        {
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                ElicaseEditorWindowKind kind;
                if (ElicaseEditorWindowThemeBridge.TryGetWindowKind(window, out kind)
                    && kind == ElicaseEditorWindowKind.Inspector)
                {
                    window.Repaint();
                }
            }
        }

        public static void DrawTitle(UnityEditor.Editor editor)
        {
            Component component;
            if (!TryGetUniformComponent(editor, out component))
            {
                return;
            }

            string title;
            if (!ElicaseInspectorComponentTitleProviders.TryGetTitle(component, out title))
            {
                return;
            }

            var headerRect = GUILayoutUtility.GetLastRect();
            DrawTitle(headerRect, title);
        }

        internal static void ApplyToInspector(VisualElement root)
        {
            if (root == null)
            {
                return;
            }

            RemoveDetachedTitles(root);
            root.Query<TextElement>().ForEach(ApplyTitleText);
            RemoveDetachedImGuiHeaders(root);
            root.Query<IMGUIContainer>().ForEach(ApplyImGuiHeader);
        }

        internal static void Restore(VisualElement root)
        {
            var titlesToRemove = new List<TextElement>();
            foreach (var pair in trackedTitles)
            {
                if (!IsDescendantOf(pair.Key, root))
                {
                    continue;
                }

                if (pair.Key.text == pair.Value.AppliedText)
                {
                    pair.Key.text = pair.Value.RawText;
                }

                titlesToRemove.Add(pair.Key);
            }

            foreach (var title in titlesToRemove)
            {
                trackedTitles.Remove(title);
            }

            var headersToRemove = new List<IMGUIContainer>();
            foreach (var pair in trackedImGuiHeaders)
            {
                if (!IsDescendantOf(pair.Key, root))
                {
                    continue;
                }

                pair.Value.Restore(pair.Key);
                headersToRemove.Add(pair.Key);
            }

            foreach (var header in headersToRemove)
            {
                trackedImGuiHeaders.Remove(header);
            }
        }

        internal static bool TryGetUniformComponent(UnityEditor.Editor editor, out Component component)
        {
            component = null;
            if (editor == null || editor.targets == null || editor.targets.Length == 0)
            {
                return false;
            }

            Type componentType = null;
            foreach (var target in editor.targets)
            {
                var candidate = target as Component;
                if (candidate == null)
                {
                    return false;
                }

                if (componentType == null)
                {
                    componentType = candidate.GetType();
                    component = candidate;
                    continue;
                }

                if (candidate.GetType() != componentType)
                {
                    component = null;
                    return false;
                }
            }

            return component != null;
        }

        private static void ApplyTitleText(TextElement element)
        {
            if (!IsComponentHeaderTitle(element))
            {
                return;
            }

            TrackedTitle tracked;
            if (!trackedTitles.TryGetValue(element, out tracked))
            {
                tracked = new TrackedTitle(element.text ?? string.Empty);
                trackedTitles.Add(element, tracked);
            }
            else if (element.text != tracked.RawText && element.text != tracked.AppliedText)
            {
                tracked.RawText = element.text ?? string.Empty;
            }

            string title;
            if (!ElicaseInspectorComponentTitleProviders.TryGetTitle(tracked.RawText, out title))
            {
                if (element.text == tracked.AppliedText)
                {
                    element.text = tracked.RawText;
                }

                trackedTitles.Remove(element);
                return;
            }

            element.text = title;
            tracked.AppliedText = title;
        }

        private static void ApplyImGuiHeader(IMGUIContainer header)
        {
            var sourceTitle = GetSourceTitle(header.name);
            if (sourceTitle == null)
            {
                return;
            }

            string title;
            if (!ElicaseInspectorComponentTitleProviders.TryGetTitle(sourceTitle, out title))
            {
                RestoreImGuiHeader(header);
                return;
            }

            TrackedImGuiHeader tracked;
            if (!trackedImGuiHeaders.TryGetValue(header, out tracked))
            {
                tracked = new TrackedImGuiHeader(header.onGUIHandler) { Title = title };
                trackedImGuiHeaders.Add(header, tracked);
                tracked.Apply(header);
                header.MarkDirtyRepaint();
            }
            else
            {
                if (!tracked.IsApplied(header))
                {
                    tracked.Apply(header);
                    header.MarkDirtyRepaint();
                }

                if (tracked.Title == title)
                {
                    return;
                }

                tracked.Title = title;
                header.MarkDirtyRepaint();
            }
        }

        private static string GetSourceTitle(string headerName)
        {
            const string headerSuffix = "Header";
            if (string.IsNullOrEmpty(headerName)
                || !headerName.EndsWith(headerSuffix, StringComparison.Ordinal))
            {
                return null;
            }

            var sourceTitle = headerName.Substring(0, headerName.Length - headerSuffix.Length).TrimEnd();
            var annotationIndex = sourceTitle.IndexOf(" (", StringComparison.Ordinal);
            if (annotationIndex < 0)
            {
                annotationIndex = sourceTitle.IndexOf(" （", StringComparison.Ordinal);
            }

            return annotationIndex < 0 ? sourceTitle : sourceTitle.Substring(0, annotationIndex);
        }

        private static void RestoreImGuiHeader(IMGUIContainer header)
        {
            TrackedImGuiHeader tracked;
            if (!trackedImGuiHeaders.TryGetValue(header, out tracked))
            {
                return;
            }

            tracked.Restore(header);
            trackedImGuiHeaders.Remove(header);
            header.MarkDirtyRepaint();
        }

        private static void RemoveDetachedImGuiHeaders(VisualElement currentRoot)
        {
            var headersToRemove = new List<IMGUIContainer>();
            foreach (var pair in trackedImGuiHeaders)
            {
                if (pair.Key.panel == null && !IsDescendantOf(pair.Key, currentRoot))
                {
                    pair.Value.Restore(pair.Key);
                    pair.Key.MarkDirtyRepaint();
                    headersToRemove.Add(pair.Key);
                }
            }

            foreach (var header in headersToRemove)
            {
                trackedImGuiHeaders.Remove(header);
            }
        }

        private static void DrawImGuiTitle(string title)
        {
            DrawTitle(GUILayoutUtility.GetLastRect(), title);
        }

        private static void DrawTitle(Rect headerRect, string title)
        {
            var titleRect = new Rect(
                headerRect.x + TitleLeftInset,
                headerRect.y + TitleVerticalInset,
                Mathf.Max(0f, headerRect.width - TitleLeftInset - TitleRightInset),
                Mathf.Min(EditorGUIUtility.singleLineHeight, Mathf.Max(0f, headerRect.height - TitleVerticalInset)));
            if (titleRect.width <= 0f || titleRect.height <= 0f)
            {
                return;
            }

            var theme = ElicaseThemeContext.FromSettings();
            var background = theme.IsDark
                ? new Color(0.235f, 0.235f, 0.235f)
                : new Color(0.76f, 0.76f, 0.76f);
            var originalContentColor = GUI.contentColor;
            EditorGUI.DrawRect(titleRect, background);
            GUI.contentColor = theme.IsDark ? Color.white : Color.black;
            GUI.Label(titleRect, title, EditorStyles.boldLabel);
            GUI.contentColor = originalContentColor;
        }

        private static bool IsComponentHeaderTitle(TextElement element)
        {
            for (VisualElement current = element; current != null; current = current.parent)
            {
                if (current.ClassListContains("unity-inspector-element__header")
                    || current.ClassListContains("unity-inspector-element__header-title"))
                {
                    return true;
                }
            }

            return false;
        }

        private static void RemoveDetachedTitles(VisualElement currentRoot)
        {
            var titlesToRemove = new List<TextElement>();
            foreach (var pair in trackedTitles)
            {
                if (pair.Key.panel == null && !IsDescendantOf(pair.Key, currentRoot))
                {
                    titlesToRemove.Add(pair.Key);
                }
            }

            foreach (var title in titlesToRemove)
            {
                trackedTitles.Remove(title);
            }
        }

        private static bool IsDescendantOf(VisualElement element, VisualElement root)
        {
            if (element == null || root == null)
            {
                return false;
            }

            for (var current = element; current != null; current = current.parent)
            {
                if (current == root)
                {
                    return true;
                }
            }

            return false;
        }

        private sealed class TrackedTitle
        {
            internal TrackedTitle(string rawText)
            {
                RawText = rawText;
                AppliedText = rawText;
            }

            internal string RawText { get; set; }
            internal string AppliedText { get; set; }
        }

        private sealed class TrackedImGuiHeader
        {
            private Action originalHandler;
            private Action appliedHandler;

            internal TrackedImGuiHeader(Action originalHandler)
            {
                this.originalHandler = originalHandler;
            }

            internal string Title { get; set; }

            internal bool IsApplied(IMGUIContainer header)
            {
                return header.onGUIHandler == appliedHandler;
            }

            internal void Apply(IMGUIContainer header)
            {
                originalHandler = header.onGUIHandler;
                appliedHandler = () =>
                {
                    originalHandler?.Invoke();
                    DrawImGuiTitle(Title);
                };
                header.onGUIHandler = appliedHandler;
            }

            internal void Restore(IMGUIContainer header)
            {
                header.onGUIHandler = originalHandler;
            }
        }
    }
}
