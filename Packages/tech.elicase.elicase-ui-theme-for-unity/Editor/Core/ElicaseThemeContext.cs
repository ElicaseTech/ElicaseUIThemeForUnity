using UnityEditor;

namespace Tech.Elicase.UITheme.Editor
{
    public sealed class ElicaseThemeContext
    {
        public ElicaseUiStyle Style { get; }
        public bool IsDark { get; }
        public bool PluginExtensionsEnabled { get; }

        public string StyleClass => Style == ElicaseUiStyle.Material
            ? ElicaseThemeTokens.ClassMaterial
            : ElicaseThemeTokens.ClassUnityOriginal;

        public string BrightnessClass => IsDark
            ? ElicaseThemeTokens.ClassDark
            : ElicaseThemeTokens.ClassLight;

        public ElicaseThemeContext(ElicaseUiStyle style, bool isDark, bool pluginExtensionsEnabled)
        {
            Style = style;
            IsDark = isDark;
            PluginExtensionsEnabled = pluginExtensionsEnabled;
        }

        public static ElicaseThemeContext FromSettings()
        {
            var settings = ElicaseThemeSettings.Current;
            var isDark = settings.FollowUnityTheme ? EditorGUIUtility.isProSkin : settings.UseDarkTheme;
            return new ElicaseThemeContext(settings.Style, isDark, settings.PluginExtensionsEnabled);
        }
    }
}
