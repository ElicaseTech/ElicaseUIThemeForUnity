using System.Collections.Generic;

namespace Tech.Elicase.UITheme.Editor
{
    public interface IElicaseEditorWindowExtension
    {
        string Id { get; }
        string DisplayName { get; }
        IReadOnlyList<ElicaseEditorWindowKind> TargetWindows { get; }
        void OnAttach(ElicaseEditorWindowContext context);
        void OnDetach(ElicaseEditorWindowContext context);
        void OnThemeChanged(ElicaseEditorWindowContext context);
    }
}
