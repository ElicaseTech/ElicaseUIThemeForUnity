using System;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public sealed class ElicaseButton : Button
    {
        public ElicaseButton(Action clicked = null, string text = "") : base(clicked)
        {
            this.text = text;
            focusable = true;
            AddToClassList(ElicaseThemeTokens.ClassButton);
        }
    }

    public sealed class ElicaseIconButton : Button
    {
        public ElicaseIconButton(Action clicked = null, Texture2D icon = null, string tooltip = "", string fallbackGlyph = "") : base(clicked)
        {
            this.tooltip = tooltip;
            focusable = true;
            AddToClassList(ElicaseThemeTokens.ClassButton);
            AddToClassList(ElicaseThemeTokens.ClassIconButton);

            if (icon != null)
            {
                Add(new Image
                {
                    image = icon,
                    scaleMode = ScaleMode.ScaleToFit
                });
                return;
            }

            if (!string.IsNullOrEmpty(fallbackGlyph))
            {
                Add(new Label(fallbackGlyph));
            }
        }
    }

    public sealed class ElicaseTextField : TextField
    {
        public ElicaseTextField(string label = null) : base(label)
        {
            AddToClassList(ElicaseThemeTokens.ClassTextField);
        }

        public void SetError(bool isError)
        {
            EnableInClassList(ElicaseThemeTokens.ClassError, isError);
        }
    }

    public sealed class ElicaseToggle : Toggle
    {
        public ElicaseToggle(string label = null) : base(label)
        {
            AddToClassList(ElicaseThemeTokens.ClassToggle);
        }
    }

    public sealed class ElicaseSelectField : PopupField<string>
    {
        public ElicaseSelectField(IList<string> choices, int defaultIndex = 0, string label = null)
            : base(label, NormalizeChoices(choices), ClampIndex(choices, defaultIndex))
        {
            AddToClassList(ElicaseThemeTokens.ClassSelect);
        }

        private static List<string> NormalizeChoices(IList<string> choices)
        {
            var normalized = choices == null ? new List<string>() : new List<string>(choices);
            if (normalized.Count == 0)
            {
                normalized.Add(string.Empty);
            }

            return normalized;
        }

        private static int ClampIndex(IList<string> choices, int defaultIndex)
        {
            var count = choices == null || choices.Count == 0 ? 1 : choices.Count;
            if (defaultIndex < 0)
            {
                return 0;
            }

            return defaultIndex >= count ? count - 1 : defaultIndex;
        }
    }

    public sealed class ElicasePanel : VisualElement
    {
        public ElicasePanel()
        {
            AddToClassList(ElicaseThemeTokens.ClassPanel);
        }
    }

    public sealed class ElicaseToolbar : Toolbar
    {
        public ElicaseToolbar()
        {
            AddToClassList(ElicaseThemeTokens.ClassToolbar);
        }
    }

    public sealed class ElicaseListRow : VisualElement
    {
        public ElicaseListRow()
        {
            focusable = true;
            AddToClassList(ElicaseThemeTokens.ClassListRow);
        }

        public void SetSelected(bool selected)
        {
            EnableInClassList(ElicaseThemeTokens.ClassListRowSelected, selected);
        }
    }

    public sealed class ElicaseDialogShell : VisualElement
    {
        public VisualElement Header { get; }
        public VisualElement Body { get; }
        public VisualElement Footer { get; }

        public ElicaseDialogShell(string title = null)
        {
            AddToClassList(ElicaseThemeTokens.ClassDialog);

            Header = new VisualElement();
            Header.AddToClassList(ElicaseThemeTokens.ClassDialogHeader);
            if (!string.IsNullOrEmpty(title))
            {
                Header.Add(new Label(title));
            }

            Body = new VisualElement();
            Body.AddToClassList(ElicaseThemeTokens.ClassDialogBody);

            Footer = new VisualElement();
            Footer.AddToClassList(ElicaseThemeTokens.ClassDialogFooter);

            Add(Header);
            Add(Body);
            Add(Footer);
        }
    }

    public sealed class ElicaseTabs : VisualElement
    {
        private readonly VisualElement tabBar;
        private readonly VisualElement contentRoot;
        private readonly List<TabItem> tabs = new List<TabItem>();

        public ElicaseTabs()
        {
            AddToClassList(ElicaseThemeTokens.ClassTabs);

            tabBar = new VisualElement();
            tabBar.AddToClassList(ElicaseThemeTokens.ClassTabBar);

            contentRoot = new VisualElement();
            contentRoot.AddToClassList(ElicaseThemeTokens.ClassTabContent);

            Add(tabBar);
            Add(contentRoot);
        }

        public void AddTab(string label, VisualElement content)
        {
            var capturedIndex = tabs.Count;
            var button = new Button(() => SelectTab(capturedIndex))
            {
                text = label,
                focusable = true
            };
            button.AddToClassList(ElicaseThemeTokens.ClassTab);

            content.style.display = DisplayStyle.None;
            contentRoot.Add(content);
            tabBar.Add(button);

            tabs.Add(new TabItem(button, content));
            if (tabs.Count == 1)
            {
                SelectTab(0);
            }
        }

        public void SelectTab(int index)
        {
            if (index < 0 || index >= tabs.Count)
            {
                return;
            }

            for (var i = 0; i < tabs.Count; i++)
            {
                var selected = i == index;
                tabs[i].Button.EnableInClassList(ElicaseThemeTokens.ClassTabSelected, selected);
                tabs[i].Content.style.display = selected ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private sealed class TabItem
        {
            public Button Button { get; }
            public VisualElement Content { get; }

            public TabItem(Button button, VisualElement content)
            {
                Button = button;
                Content = content;
            }
        }
    }
}
