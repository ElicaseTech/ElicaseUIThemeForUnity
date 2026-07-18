using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    [InitializeOnLoad]
    public static class ElicaseEditorWindowThemeBridge
    {
        private const string HierarchyWindowTypeName = "UnityEditor.SceneHierarchyWindow";
        private const string InspectorWindowTypeName = "UnityEditor.InspectorWindow";
        private const double RefreshIntervalSeconds = 0.5d;

        private static readonly Dictionary<int, WindowAttachment> attachments =
            new Dictionary<int, WindowAttachment>();

        private static double nextRefreshTime;
        private static bool refreshScheduled;

        static ElicaseEditorWindowThemeBridge()
        {
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += DetachAll;
            ElicaseUiEvents.ThemeChanged += HandleThemeChanged;
            RequestRefresh();
        }

        public static bool TryGetWindowKind(EditorWindow window, out ElicaseEditorWindowKind kind)
        {
            kind = default(ElicaseEditorWindowKind);
            if (window == null)
            {
                return false;
            }

            var typeName = window.GetType().FullName;
            if (typeName == HierarchyWindowTypeName)
            {
                kind = ElicaseEditorWindowKind.Hierarchy;
                return true;
            }

            if (typeName == InspectorWindowTypeName)
            {
                kind = ElicaseEditorWindowKind.Inspector;
                return true;
            }

            return false;
        }

        internal static void RequestRefresh()
        {
            if (refreshScheduled)
            {
                return;
            }

            refreshScheduled = true;
            EditorApplication.delayCall += RefreshNow;
        }

        private static void Update()
        {
            if (EditorApplication.timeSinceStartup < nextRefreshTime)
            {
                return;
            }

            nextRefreshTime = EditorApplication.timeSinceStartup + RefreshIntervalSeconds;
            RefreshNow();
        }

        private static void RefreshNow()
        {
            refreshScheduled = false;
            Refresh(ElicaseThemeContext.FromSettings(), false);
        }

        private static void HandleThemeChanged(ElicaseThemeContext context)
        {
            Refresh(context, true);
        }

        private static void Refresh(ElicaseThemeContext theme, bool notifyThemeChanged)
        {
            var activeWindowIds = new HashSet<int>();
            foreach (var window in Resources.FindObjectsOfTypeAll<EditorWindow>())
            {
                ElicaseEditorWindowKind kind;
                if (!TryGetWindowKind(window, out kind))
                {
                    continue;
                }

                var root = window.rootVisualElement;
                if (root == null)
                {
                    continue;
                }

                var windowId = window.GetInstanceID();
                activeWindowIds.Add(windowId);

                WindowAttachment attachment;
                if (attachments.TryGetValue(windowId, out attachment) && !attachment.IsCurrent(window, root))
                {
                    Detach(windowId, attachment, theme);
                    attachment = null;
                }

                if (attachment == null)
                {
                    attachment = CreateAttachment(window, root, kind, theme);
                    attachments[windowId] = attachment;
                }
                else if (ElicaseThemeSettings.Current.BuiltInWindowThemingEnabled)
                {
                    ElicaseThemeManager.ApplyWindowContent(window, theme);
                }
                else
                {
                    ElicaseThemeManager.RemoveWindowContent(window);
                }

                SynchronizeExtensions(attachment, theme, notifyThemeChanged);
                SynchronizeObservers(attachment, theme);
            }

            var staleWindowIds = new List<int>();
            foreach (var pair in attachments)
            {
                if (!activeWindowIds.Contains(pair.Key))
                {
                    staleWindowIds.Add(pair.Key);
                }
            }

            foreach (var windowId in staleWindowIds)
            {
                Detach(windowId, attachments[windowId], theme);
            }
        }

        private static WindowAttachment CreateAttachment(
            EditorWindow window,
            VisualElement root,
            ElicaseEditorWindowKind kind,
            ElicaseThemeContext theme)
        {
            if (ElicaseThemeSettings.Current.BuiltInWindowThemingEnabled)
            {
                ElicaseThemeManager.ApplyWindowContent(window, theme);
            }

            return new WindowAttachment(window, root, kind);
        }

        private static void SynchronizeExtensions(
            WindowAttachment attachment,
            ElicaseThemeContext theme,
            bool notifyThemeChanged)
        {
            var shouldAttachExtensions = ElicaseThemeSettings.Current.BuiltInWindowThemingEnabled
                && ElicaseThemeSettings.Current.PluginExtensionsEnabled;
            var registeredExtensions = new List<IElicaseEditorWindowExtension>(
                ElicaseEditorWindowExtensions.RegisteredExtensions);

            var attachedExtensions = new List<IElicaseEditorWindowExtension>(attachment.Extensions);
            foreach (var extension in attachedExtensions)
            {
                if (!shouldAttachExtensions || !Contains(registeredExtensions, extension) || !Supports(extension, attachment.Kind))
                {
                    DetachExtension(attachment, extension, theme);
                }
            }

            if (!shouldAttachExtensions)
            {
                return;
            }

            foreach (var extension in registeredExtensions)
            {
                if (!Supports(extension, attachment.Kind))
                {
                    continue;
                }

                if (!attachment.Extensions.Contains(extension))
                {
                    extension.OnAttach(CreateContext(attachment, theme));
                    attachment.Extensions.Add(extension);
                }

                if (notifyThemeChanged)
                {
                    extension.OnThemeChanged(CreateContext(attachment, theme));
                }
            }
        }

        private static void SynchronizeObservers(WindowAttachment attachment, ElicaseThemeContext theme)
        {
            var registeredObservers = new List<IElicaseEditorWindowObserver>(
                ElicaseEditorWindowObservers.RegisteredObservers);
            var attachedObservers = new List<IElicaseEditorWindowObserver>(attachment.Observers);
            foreach (var observer in attachedObservers)
            {
                if (!Contains(registeredObservers, observer) || !Supports(observer, attachment.Kind))
                {
                    DetachObserver(attachment, observer, theme);
                }
            }

            foreach (var observer in registeredObservers)
            {
                if (!Supports(observer, attachment.Kind))
                {
                    continue;
                }

                if (!attachment.Observers.Contains(observer))
                {
                    observer.OnAttach(CreateContext(attachment, theme));
                    attachment.Observers.Add(observer);
                }

                observer.OnRefresh(CreateContext(attachment, theme));
            }
        }

        private static bool Contains(IReadOnlyList<IElicaseEditorWindowExtension> extensions, IElicaseEditorWindowExtension extension)
        {
            foreach (var registeredExtension in extensions)
            {
                if (ReferenceEquals(registeredExtension, extension))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Contains(IReadOnlyList<IElicaseEditorWindowObserver> observers, IElicaseEditorWindowObserver observer)
        {
            foreach (var registeredObserver in observers)
            {
                if (ReferenceEquals(registeredObserver, observer))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Supports(IElicaseEditorWindowExtension extension, ElicaseEditorWindowKind kind)
        {
            if (extension.TargetWindows == null)
            {
                return false;
            }

            foreach (var target in extension.TargetWindows)
            {
                if (target == kind)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool Supports(IElicaseEditorWindowObserver observer, ElicaseEditorWindowKind kind)
        {
            if (observer.TargetWindows == null)
            {
                return false;
            }

            foreach (var target in observer.TargetWindows)
            {
                if (target == kind)
                {
                    return true;
                }
            }

            return false;
        }

        private static void DetachAll()
        {
            var theme = ElicaseThemeContext.FromSettings();
            var existingAttachments = new List<KeyValuePair<int, WindowAttachment>>(attachments);
            foreach (var pair in existingAttachments)
            {
                Detach(pair.Key, pair.Value, theme);
            }
        }

        private static void Detach(int windowId, WindowAttachment attachment, ElicaseThemeContext theme)
        {
            var extensions = new List<IElicaseEditorWindowExtension>(attachment.Extensions);
            foreach (var extension in extensions)
            {
                DetachExtension(attachment, extension, theme);
            }

            var observers = new List<IElicaseEditorWindowObserver>(attachment.Observers);
            foreach (var observer in observers)
            {
                DetachObserver(attachment, observer, theme);
            }

            attachment.Dispose();
            ElicaseThemeManager.RemoveWindowContent(attachment.Root);
            attachments.Remove(windowId);
        }

        private static void DetachExtension(
            WindowAttachment attachment,
            IElicaseEditorWindowExtension extension,
            ElicaseThemeContext theme)
        {
            extension.OnDetach(CreateContext(attachment, theme));
            attachment.Extensions.Remove(extension);
        }

        private static void DetachObserver(
            WindowAttachment attachment,
            IElicaseEditorWindowObserver observer,
            ElicaseThemeContext theme)
        {
            observer.OnDetach(CreateContext(attachment, theme));
            attachment.Observers.Remove(observer);
        }

        private static ElicaseEditorWindowContext CreateContext(WindowAttachment attachment, ElicaseThemeContext theme)
        {
            return new ElicaseEditorWindowContext(attachment.Kind, attachment.Window, attachment.Root, theme);
        }

        private sealed class WindowAttachment
        {
            public EditorWindow Window { get; }
            public VisualElement Root { get; }
            public ElicaseEditorWindowKind Kind { get; }
            public List<IElicaseEditorWindowExtension> Extensions { get; } =
                new List<IElicaseEditorWindowExtension>();
            public List<IElicaseEditorWindowObserver> Observers { get; } =
                new List<IElicaseEditorWindowObserver>();

            private readonly EventCallback<AttachToPanelEvent> attachToPanelCallback;
            private readonly EventCallback<DetachFromPanelEvent> detachFromPanelCallback;

            public WindowAttachment(
                EditorWindow window,
                VisualElement root,
                ElicaseEditorWindowKind kind)
            {
                Window = window;
                Root = root;
                Kind = kind;
                attachToPanelCallback = _ => RequestRefresh();
                detachFromPanelCallback = _ => RequestRefresh();
                Root.RegisterCallback(attachToPanelCallback);
                Root.RegisterCallback(detachFromPanelCallback);
            }

            public void Dispose()
            {
                Root.UnregisterCallback(attachToPanelCallback);
                Root.UnregisterCallback(detachFromPanelCallback);
            }

            public bool IsCurrent(EditorWindow window, VisualElement root)
            {
                return ReferenceEquals(Window, window)
                    && ReferenceEquals(Root, root);
            }
        }
    }
}
