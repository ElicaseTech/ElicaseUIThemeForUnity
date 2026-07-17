using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public interface IElicaseUiExtension
    {
        string Id { get; }
        string DisplayName { get; }
        VisualElement CreateContent();
        void OnThemeChanged(ElicaseThemeContext context);
    }
}
