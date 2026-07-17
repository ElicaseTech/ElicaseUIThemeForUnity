using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public sealed class ElicaseThemePreviewWindow : EditorWindow
    {
        private BuiltInWindowPreviewExtension builtInWindowExtension;
        private bool builtInWindowExtensionRegistered;

        [MenuItem("Window/Elicase/UI Theme Preview")]
        public static void Open()
        {
            var window = GetWindow<ElicaseThemePreviewWindow>();
            window.titleContent = new UnityEngine.GUIContent("Elicase UI Theme");
            window.minSize = new UnityEngine.Vector2(420f, 360f);
        }

        public void CreateGUI()
        {
            ElicaseThemeManager.Apply(rootVisualElement);

            var toolbar = new ElicaseToolbar();
            toolbar.Add(new Label("Elicase UI Theme"));
            toolbar.Add(new ElicaseButton(() => ElicaseUiEvents.NotifyThemeChanged(), "Refresh"));

            var panel = new ElicasePanel();
            panel.Add(new Label("Material UI Toolkit Controls"));
            panel.Add(new ElicaseTextField("Text Field") { value = "Editor-ready input" });
            panel.Add(new ElicaseToggle("Toggle") { value = true });
            panel.Add(new ElicaseSelectField(new List<string> { "Material", "Unity Original" }, 0, "Select"));

            var tabs = new ElicaseTabs();
            tabs.AddTab("Components", panel);

            var pluginPanel = new ElicasePanel();
            pluginPanel.Add(new ElicasePluginUiHost(new PreviewExtension()));
            tabs.AddTab("Plugin Host", pluginPanel);

            var builtInWindowPanel = new ElicasePanel();
            var builtInWindowToggle = new ElicaseToggle("Attach preview extension to Hierarchy and Inspector")
            {
                value = builtInWindowExtensionRegistered
            };
            builtInWindowToggle.RegisterValueChangedCallback(evt => SetBuiltInWindowExtensionRegistered(evt.newValue));
            builtInWindowPanel.Add(builtInWindowToggle);
            tabs.AddTab("Built-in Windows", builtInWindowPanel);

            var dialog = new ElicaseDialogShell("Dialog Shell");
            dialog.Body.Add(new Label("Reusable header, body and footer regions for upper-layer plugins."));
            dialog.Footer.Add(new ElicaseButton(null, "Cancel"));
            dialog.Footer.Add(new ElicaseButton(null, "Apply"));

            rootVisualElement.Add(toolbar);
            rootVisualElement.Add(tabs);
            rootVisualElement.Add(dialog);

            ElicaseUiEvents.ThemeChanged += HandleThemeChanged;
        }

        private void OnDisable()
        {
            SetBuiltInWindowExtensionRegistered(false);
            ElicaseUiEvents.ThemeChanged -= HandleThemeChanged;
        }

        private void SetBuiltInWindowExtensionRegistered(bool registered)
        {
            if (registered == builtInWindowExtensionRegistered)
            {
                return;
            }

            if (registered)
            {
                builtInWindowExtension = new BuiltInWindowPreviewExtension();
                ElicaseEditorWindowExtensions.Register(builtInWindowExtension);
            }
            else if (builtInWindowExtension != null)
            {
                ElicaseEditorWindowExtensions.Unregister(builtInWindowExtension);
                builtInWindowExtension = null;
            }

            builtInWindowExtensionRegistered = registered;
        }

        private void HandleThemeChanged(ElicaseThemeContext context)
        {
            ElicaseThemeManager.Apply(rootVisualElement, context);
        }

        private sealed class PreviewExtension : IElicaseUiExtension
        {
            public string Id => "tech.elicase.preview";
            public string DisplayName => "Preview Extension";

            public VisualElement CreateContent()
            {
                var panel = new ElicasePanel();
                panel.Add(new Label(DisplayName));
                panel.Add(new ElicaseButton(null, "Plugin Action"));
                return panel;
            }

            public void OnThemeChanged(ElicaseThemeContext context)
            {
            }
        }

        private sealed class BuiltInWindowPreviewExtension : IElicaseEditorWindowExtension
        {
            private const string PanelName = "elicase-preview-built-in-window-extension";
            private static readonly IReadOnlyList<ElicaseEditorWindowKind> TargetWindowKinds =
                new List<ElicaseEditorWindowKind>
                {
                    ElicaseEditorWindowKind.Hierarchy,
                    ElicaseEditorWindowKind.Inspector
                };

            public string Id => "tech.elicase.preview.built-in-window";
            public string DisplayName => "Built-in Window Preview Extension";
            public IReadOnlyList<ElicaseEditorWindowKind> TargetWindows => TargetWindowKinds;

            public void OnAttach(ElicaseEditorWindowContext context)
            {
                var panel = new ElicasePanel
                {
                    name = PanelName
                };
                panel.style.position = Position.Absolute;
                panel.style.right = 8f;
                panel.style.bottom = 8f;
                panel.style.width = 180f;
                panel.Add(new Label(DisplayName));
                panel.Add(new Label(context.Kind.ToString()));
                context.RootVisualElement.Add(panel);
            }

            public void OnDetach(ElicaseEditorWindowContext context)
            {
                var panel = context.RootVisualElement.Q<VisualElement>(PanelName);
                panel?.RemoveFromHierarchy();
            }

            public void OnThemeChanged(ElicaseEditorWindowContext context)
            {
                var panel = context.RootVisualElement.Q<VisualElement>(PanelName);
                if (panel != null)
                {
                    ElicaseThemeManager.Apply(panel, context.Theme);
                }
            }
        }
    }
}
