using System;
using System.Collections.Generic;

namespace Tech.Elicase.UITheme.Editor
{
    public static class ElicaseEditorWindowObservers
    {
        private static readonly List<IElicaseEditorWindowObserver> observers =
            new List<IElicaseEditorWindowObserver>();

        internal static IReadOnlyList<IElicaseEditorWindowObserver> RegisteredObservers => observers;

        public static void Register(IElicaseEditorWindowObserver observer)
        {
            if (observer == null)
            {
                throw new ArgumentNullException(nameof(observer));
            }

            if (string.IsNullOrWhiteSpace(observer.Id))
            {
                throw new ArgumentException("Window observers require a non-empty id.", nameof(observer));
            }

            foreach (var registeredObserver in observers)
            {
                if (registeredObserver.Id == observer.Id)
                {
                    throw new ArgumentException("A window observer with the same id is already registered.", nameof(observer));
                }
            }

            observers.Add(observer);
            RequestRefresh();
        }

        public static bool Unregister(IElicaseEditorWindowObserver observer)
        {
            if (observer == null || !observers.Remove(observer))
            {
                return false;
            }

            RequestRefresh();
            return true;
        }

        public static void RequestRefresh()
        {
            ElicaseEditorWindowThemeBridge.RequestRefresh();
        }
    }
}
