using System;
using System.Collections.Generic;

namespace Tech.Elicase.UITheme.Editor
{
    public static class ElicaseEditorWindowExtensions
    {
        private static readonly List<IElicaseEditorWindowExtension> extensions =
            new List<IElicaseEditorWindowExtension>();

        internal static IReadOnlyList<IElicaseEditorWindowExtension> RegisteredExtensions => extensions;

        public static void Register(IElicaseEditorWindowExtension extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (string.IsNullOrWhiteSpace(extension.Id))
            {
                throw new ArgumentException("Window extensions require a non-empty id.", nameof(extension));
            }

            foreach (var registeredExtension in extensions)
            {
                if (registeredExtension.Id == extension.Id)
                {
                    throw new ArgumentException("A window extension with the same id is already registered.", nameof(extension));
                }
            }

            extensions.Add(extension);
            ElicaseEditorWindowThemeBridge.RequestRefresh();
        }

        public static bool Unregister(IElicaseEditorWindowExtension extension)
        {
            if (extension == null || !extensions.Remove(extension))
            {
                return false;
            }

            ElicaseEditorWindowThemeBridge.RequestRefresh();
            return true;
        }
    }
}
