using UnityEngine;

namespace Tech.Elicase.UITheme.Editor
{
    /// <summary>
    /// Provides an optional replacement title for a component in Unity's Inspector header.
    /// </summary>
    public interface IElicaseInspectorComponentTitleProvider
    {
        string Id { get; }
        bool TryGetTitle(Component component, out string title);
    }

    /// <summary>
    /// Resolves Inspector component-header text when Unity renders the header with UI Toolkit.
    /// </summary>
    public interface IElicaseInspectorTitleTextProvider
    {
        bool TryGetTitle(string sourceTitle, out string title);
    }
}
