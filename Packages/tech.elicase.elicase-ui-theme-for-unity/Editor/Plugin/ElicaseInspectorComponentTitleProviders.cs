using System;
using System.Collections.Generic;
using UnityEngine;

namespace Tech.Elicase.UITheme.Editor
{
    /// <summary>
    /// Registers component-title providers used by the built-in Inspector header renderer.
    /// </summary>
    public static class ElicaseInspectorComponentTitleProviders
    {
        private static readonly List<IElicaseInspectorComponentTitleProvider> providers =
            new List<IElicaseInspectorComponentTitleProvider>();

        public static void Register(IElicaseInspectorComponentTitleProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (string.IsNullOrWhiteSpace(provider.Id))
            {
                throw new ArgumentException("Component title providers require a non-empty id.", nameof(provider));
            }

            foreach (var registeredProvider in providers)
            {
                if (registeredProvider.Id == provider.Id)
                {
                    throw new ArgumentException("A component title provider with the same id is already registered.", nameof(provider));
                }
            }

            providers.Add(provider);
            RequestRefresh();
        }

        public static bool Unregister(IElicaseInspectorComponentTitleProvider provider)
        {
            if (provider == null || !providers.Remove(provider))
            {
                return false;
            }

            RequestRefresh();
            return true;
        }

        public static bool TryGetTitle(Component component, out string title)
        {
            if (component != null)
            {
                foreach (var provider in providers)
                {
                    string candidate;
                    if (provider.TryGetTitle(component, out candidate) && !string.IsNullOrWhiteSpace(candidate))
                    {
                        title = candidate;
                        return true;
                    }
                }
            }

            title = null;
            return false;
        }

        public static bool TryGetTitle(string sourceTitle, out string title)
        {
            if (!string.IsNullOrEmpty(sourceTitle))
            {
                foreach (var provider in providers)
                {
                    var textProvider = provider as IElicaseInspectorTitleTextProvider;
                    string candidate;
                    if (textProvider != null
                        && textProvider.TryGetTitle(sourceTitle, out candidate)
                        && !string.IsNullOrWhiteSpace(candidate))
                    {
                        title = candidate;
                        return true;
                    }
                }
            }

            title = null;
            return false;
        }

        public static void RequestRefresh()
        {
            ElicaseInspectorHeaderRenderer.RequestRefresh();
        }
    }
}
