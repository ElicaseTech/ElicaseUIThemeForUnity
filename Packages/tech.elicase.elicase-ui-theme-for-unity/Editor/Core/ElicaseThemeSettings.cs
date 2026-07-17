using UnityEditor;
using UnityEngine;

namespace Tech.Elicase.UITheme.Editor
{
    [FilePath("ProjectSettings/ElicaseUIThemeSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public sealed class ElicaseThemeSettings : ScriptableSingleton<ElicaseThemeSettings>
    {
        [SerializeField] private ElicaseUiStyle style = ElicaseUiStyle.Material;
        [SerializeField] private bool followUnityTheme = true;
        [SerializeField] private bool useDarkTheme = true;
        [SerializeField] private bool pluginExtensionsEnabled = true;
        [SerializeField] private bool builtInWindowThemingEnabled = true;

        public static ElicaseThemeSettings Current => instance;

        public ElicaseUiStyle Style
        {
            get => style;
            set
            {
                if (style == value)
                {
                    return;
                }

                style = value;
                SaveAndNotify();
            }
        }

        public bool FollowUnityTheme
        {
            get => followUnityTheme;
            set
            {
                if (followUnityTheme == value)
                {
                    return;
                }

                followUnityTheme = value;
                SaveAndNotify();
            }
        }

        public bool UseDarkTheme
        {
            get => useDarkTheme;
            set
            {
                if (useDarkTheme == value)
                {
                    return;
                }

                useDarkTheme = value;
                SaveAndNotify();
            }
        }

        public bool PluginExtensionsEnabled
        {
            get => pluginExtensionsEnabled;
            set
            {
                if (pluginExtensionsEnabled == value)
                {
                    return;
                }

                pluginExtensionsEnabled = value;
                SaveAndNotify();
            }
        }

        public bool BuiltInWindowThemingEnabled
        {
            get => builtInWindowThemingEnabled;
            set
            {
                if (builtInWindowThemingEnabled == value)
                {
                    return;
                }

                builtInWindowThemingEnabled = value;
                SaveAndNotify();
            }
        }

        public void SaveAndNotify()
        {
            Save(true);
            ElicaseUiEvents.RaiseThemeChanged(ElicaseThemeContext.FromSettings());
        }
    }
}
