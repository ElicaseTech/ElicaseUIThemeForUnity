using System;

namespace Tech.Elicase.UITheme.Editor
{
    public static class ElicaseUiEvents
    {
        public static event Action<ElicaseThemeContext> ThemeChanged;

        internal static void RaiseThemeChanged(ElicaseThemeContext context)
        {
            ThemeChanged?.Invoke(context);
        }

        public static void NotifyThemeChanged()
        {
            RaiseThemeChanged(ElicaseThemeContext.FromSettings());
        }
    }
}
