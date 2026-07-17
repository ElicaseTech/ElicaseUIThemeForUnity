namespace Tech.Elicase.UITheme.Editor
{
    public readonly struct ElicaseThemeColorTokens
    {
        public readonly string Background;
        public readonly string Surface;
        public readonly string SurfaceVariant;
        public readonly string Outline;
        public readonly string Primary;
        public readonly string OnSurface;
        public readonly string OnPrimary;
        public readonly string Error;

        public ElicaseThemeColorTokens(
            string background,
            string surface,
            string surfaceVariant,
            string outline,
            string primary,
            string onSurface,
            string onPrimary,
            string error)
        {
            Background = background;
            Surface = surface;
            SurfaceVariant = surfaceVariant;
            Outline = outline;
            Primary = primary;
            OnSurface = onSurface;
            OnPrimary = onPrimary;
            Error = error;
        }
    }

    public static class ElicaseThemeTokens
    {
        public const string PackagePath = "Packages/tech.elicase.elicase-ui-theme-for-unity";

        public const string ClassRoot = "elicase-ui";
        public const string ClassMaterial = "elicase-style-material";
        public const string ClassUnityOriginal = "elicase-style-unity-original";
        public const string ClassDark = "elicase-theme-dark";
        public const string ClassLight = "elicase-theme-light";

        public const string ClassPanel = "elicase-panel";
        public const string ClassToolbar = "elicase-toolbar";
        public const string ClassButton = "elicase-button";
        public const string ClassIconButton = "elicase-icon-button";
        public const string ClassTextField = "elicase-text-field";
        public const string ClassToggle = "elicase-toggle";
        public const string ClassSelect = "elicase-select";
        public const string ClassTabs = "elicase-tabs";
        public const string ClassTabBar = "elicase-tab-bar";
        public const string ClassTab = "elicase-tab";
        public const string ClassTabSelected = "elicase-tab-selected";
        public const string ClassTabContent = "elicase-tab-content";
        public const string ClassListRow = "elicase-list-row";
        public const string ClassListRowSelected = "elicase-list-row-selected";
        public const string ClassDialog = "elicase-dialog";
        public const string ClassDialogHeader = "elicase-dialog-header";
        public const string ClassDialogBody = "elicase-dialog-body";
        public const string ClassDialogFooter = "elicase-dialog-footer";
        public const string ClassPluginHost = "elicase-plugin-host";
        public const string ClassPluginToolbar = "elicase-plugin-toolbar";
        public const string ClassPluginContent = "elicase-plugin-content";
        public const string ClassPluginDialogLayer = "elicase-plugin-dialog-layer";
        public const string ClassError = "elicase-error";
        public const string ClassWindowContent = "elicase-window-content";
        public const string ClassWindowHierarchy = "elicase-window-hierarchy";
        public const string ClassWindowInspector = "elicase-window-inspector";

        public const float CornerSmall = 4f;
        public const float CornerMedium = 8f;
        public const float CornerLarge = 12f;
        public const float StateLayerHoverOpacity = 0.08f;
        public const float StateLayerPressedOpacity = 0.12f;
        public const float StateLayerFocusOpacity = 0.12f;

        public static readonly string[] CommonStyleSheetPaths =
        {
            PackagePath + "/Editor/Resources/ElicaseUITheme/tokens.uss",
            PackagePath + "/Editor/Resources/ElicaseUITheme/components.uss"
        };

        public const string WindowContentStyleSheetPath =
            PackagePath + "/Editor/Resources/ElicaseUITheme/editor-window-content.uss";

        public static string GetVariantStyleSheetPath(ElicaseUiStyle style)
        {
            return style == ElicaseUiStyle.Material
                ? PackagePath + "/Editor/Resources/ElicaseUITheme/material.uss"
                : PackagePath + "/Editor/Resources/ElicaseUITheme/unity-original.uss";
        }

        public static ElicaseThemeColorTokens GetColors(bool isDark)
        {
            return isDark
                ? new ElicaseThemeColorTokens("#1E1E1E", "#2A2A2A", "#3A3A3A", "#555555", "#4C9EFF", "#DCDCDC", "#FFFFFF", "#F05D5E")
                : new ElicaseThemeColorTokens("#CFCFCF", "#E5E5E5", "#DADADA", "#A5A5A5", "#006FC9", "#202020", "#FFFFFF", "#B3261E");
        }
    }
}
