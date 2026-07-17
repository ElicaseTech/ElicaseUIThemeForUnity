using UnityEditor;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public sealed class ElicaseEditorWindowContext
    {
        public ElicaseEditorWindowKind Kind { get; }
        public EditorWindow Window { get; }
        public VisualElement RootVisualElement { get; }
        public ElicaseThemeContext Theme { get; }

        internal ElicaseEditorWindowContext(
            ElicaseEditorWindowKind kind,
            EditorWindow window,
            VisualElement rootVisualElement,
            ElicaseThemeContext theme)
        {
            Kind = kind;
            Window = window;
            RootVisualElement = rootVisualElement;
            Theme = theme;
        }
    }
}
