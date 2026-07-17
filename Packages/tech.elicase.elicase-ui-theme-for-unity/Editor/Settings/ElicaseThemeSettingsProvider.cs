using UnityEditor;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public static class ElicaseThemeSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new SettingsProvider("Project/Elicase UI Theme", SettingsScope.Project)
            {
                label = "Elicase UI Theme",
                activateHandler = (_, root) => Build(root),
                keywords = new[] { "Elicase", "UI", "Theme", "Material", "Toolkit" }
            };
        }

        private static void Build(VisualElement root)
        {
            root.Clear();
            ElicaseThemeManager.Apply(root);

            var settings = ElicaseThemeSettings.Current;

            var styleField = new EnumField("UI Style", settings.Style);
            styleField.RegisterValueChangedCallback(evt =>
            {
                settings.Style = (ElicaseUiStyle)evt.newValue;
                ElicaseThemeManager.Apply(root);
            });

            var followUnityThemeField = new Toggle("Follow Unity Theme")
            {
                value = settings.FollowUnityTheme
            };
            followUnityThemeField.RegisterValueChangedCallback(evt =>
            {
                settings.FollowUnityTheme = evt.newValue;
                ElicaseThemeManager.Apply(root);
            });

            var darkThemeField = new Toggle("Use Dark Theme")
            {
                value = settings.UseDarkTheme
            };
            darkThemeField.SetEnabled(!settings.FollowUnityTheme);
            darkThemeField.RegisterValueChangedCallback(evt =>
            {
                settings.UseDarkTheme = evt.newValue;
                ElicaseThemeManager.Apply(root);
            });

            followUnityThemeField.RegisterValueChangedCallback(evt => darkThemeField.SetEnabled(!evt.newValue));

            var pluginExtensionsField = new Toggle("Enable Plugin Extensions")
            {
                value = settings.PluginExtensionsEnabled
            };
            pluginExtensionsField.RegisterValueChangedCallback(evt => settings.PluginExtensionsEnabled = evt.newValue);

            var builtInWindowThemingField = new Toggle("Enable Built-in Window Theming")
            {
                value = settings.BuiltInWindowThemingEnabled
            };
            builtInWindowThemingField.RegisterValueChangedCallback(evt => settings.BuiltInWindowThemingEnabled = evt.newValue);

            var panel = new ElicasePanel();
            panel.Add(new Label("Theme"));
            panel.Add(styleField);
            panel.Add(followUnityThemeField);
            panel.Add(darkThemeField);
            panel.Add(pluginExtensionsField);
            panel.Add(builtInWindowThemingField);

            root.Add(panel);
        }
    }
}
