using System.Collections.Generic;

namespace Tech.Elicase.UITheme.Editor
{
    /// <summary>
    /// Observes Unity built-in editor windows independently from visual theme settings.
    /// </summary>
    public interface IElicaseEditorWindowObserver
    {
        string Id { get; }
        IReadOnlyList<ElicaseEditorWindowKind> TargetWindows { get; }
        void OnAttach(ElicaseEditorWindowContext context);
        void OnRefresh(ElicaseEditorWindowContext context);
        void OnDetach(ElicaseEditorWindowContext context);
    }
}
