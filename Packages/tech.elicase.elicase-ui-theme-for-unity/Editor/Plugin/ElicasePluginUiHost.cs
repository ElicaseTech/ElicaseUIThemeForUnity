using System;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public sealed class ElicasePluginUiHost : VisualElement, IDisposable
    {
        private readonly IElicaseUiExtension extension;

        public VisualElement Toolbar { get; }
        public VisualElement Content { get; }
        public VisualElement DialogLayer { get; }

        public ElicasePluginUiHost(IElicaseUiExtension extension, ElicaseThemeContext context = null)
        {
            this.extension = extension ?? throw new ArgumentNullException(nameof(extension));

            AddToClassList(ElicaseThemeTokens.ClassPluginHost);

            Toolbar = new ElicaseToolbar();
            Toolbar.AddToClassList(ElicaseThemeTokens.ClassPluginToolbar);

            Content = new VisualElement();
            Content.AddToClassList(ElicaseThemeTokens.ClassPluginContent);

            DialogLayer = new VisualElement();
            DialogLayer.AddToClassList(ElicaseThemeTokens.ClassPluginDialogLayer);

            Add(Toolbar);
            Add(Content);
            Add(DialogLayer);

            ApplyExtensionContent();
            ElicaseThemeManager.Apply(this, context);
            ElicaseUiEvents.ThemeChanged += HandleThemeChanged;
        }

        public void Dispose()
        {
            ElicaseUiEvents.ThemeChanged -= HandleThemeChanged;
        }

        private void ApplyExtensionContent()
        {
            Content.Clear();

            if (!ElicaseThemeSettings.Current.PluginExtensionsEnabled)
            {
                Content.Add(new Label("Plugin extensions are disabled."));
                return;
            }

            var content = extension.CreateContent();
            if (content != null)
            {
                Content.Add(content);
            }
        }

        private void HandleThemeChanged(ElicaseThemeContext context)
        {
            ElicaseThemeManager.Apply(this, context);
            extension.OnThemeChanged(context);
        }
    }
}
