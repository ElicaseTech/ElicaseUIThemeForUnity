using System;
using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.TestTools;

namespace Tech.Elicase.UITheme.Editor.InspectorTests
{
    public sealed class ElicaseInspectorComponentTitleProvidersTests
    {
        [Test]
        public void RegisterRejectsDuplicateProviderIds()
        {
            var first = new TestProvider("test.inspector-title.duplicate", true, "First");
            var duplicate = new TestProvider("test.inspector-title.duplicate", true, "Duplicate");

            ElicaseInspectorComponentTitleProviders.Register(first);
            try
            {
                Assert.Throws<ArgumentException>(() => ElicaseInspectorComponentTitleProviders.Register(duplicate));
            }
            finally
            {
                ElicaseInspectorComponentTitleProviders.Unregister(first);
            }
        }

        [Test]
        public void TryGetTitleUsesTheFirstProviderWithATitle()
        {
            var blank = new TestProvider("test.inspector-title.blank", true, " ");
            var first = new TestProvider("test.inspector-title.first", true, "First title");
            var later = new TestProvider("test.inspector-title.later", true, "Later title");
            var gameObject = new GameObject("Inspector Title Provider Test");

            ElicaseInspectorComponentTitleProviders.Register(blank);
            ElicaseInspectorComponentTitleProviders.Register(first);
            ElicaseInspectorComponentTitleProviders.Register(later);
            try
            {
                string title;
                Assert.That(
                    ElicaseInspectorComponentTitleProviders.TryGetTitle(gameObject.AddComponent<InspectorTitleTestComponent>(), out title),
                    Is.True);
                Assert.That(title, Is.EqualTo("First title"));
            }
            finally
            {
                ElicaseInspectorComponentTitleProviders.Unregister(later);
                ElicaseInspectorComponentTitleProviders.Unregister(first);
                ElicaseInspectorComponentTitleProviders.Unregister(blank);
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void UnregisterRestoresTheNativeTitlePath()
        {
            var provider = new TestProvider("test.inspector-title.unregister", true, "Translated title");
            var gameObject = new GameObject("Inspector Title Provider Test");

            ElicaseInspectorComponentTitleProviders.Register(provider);
            try
            {
                var component = gameObject.AddComponent<InspectorTitleTestComponent>();
                string title;
                Assert.That(ElicaseInspectorComponentTitleProviders.TryGetTitle(component, out title), Is.True);
                Assert.That(ElicaseInspectorComponentTitleProviders.Unregister(provider), Is.True);
                Assert.That(ElicaseInspectorComponentTitleProviders.TryGetTitle(component, out title), Is.False);
                Assert.That(title, Is.Null);
            }
            finally
            {
                ElicaseInspectorComponentTitleProviders.Unregister(provider);
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void UniformComponentLookupAcceptsMultiSelectionOfTheSameType()
        {
            var firstGameObject = new GameObject("Inspector Title First Selection");
            var secondGameObject = new GameObject("Inspector Title Second Selection");
            var first = firstGameObject.AddComponent<InspectorTitleTestComponent>();
            var second = secondGameObject.AddComponent<InspectorTitleTestComponent>();
            var editor = UnityEditor.Editor.CreateEditor(new UnityEngine.Object[] { first, second });

            try
            {
                Component component;
                Assert.That(ElicaseInspectorHeaderRenderer.TryGetUniformComponent(editor, out component), Is.True);
                Assert.That(component, Is.SameAs(first));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(editor);
                UnityEngine.Object.DestroyImmediate(firstGameObject);
                UnityEngine.Object.DestroyImmediate(secondGameObject);
            }
        }

        [Test]
        public void UiToolkitHeaderAppliesAndRestoresATranslatedTitle()
        {
            var provider = new TextProvider(
                "test.inspector-title.ui-toolkit",
                "Source Component",
                "中文组件");
            var root = new VisualElement();
            var header = new VisualElement();
            header.AddToClassList("unity-inspector-element__header-title");
            var title = new Label("Source Component");
            header.Add(title);
            root.Add(header);

            ElicaseInspectorComponentTitleProviders.Register(provider);
            try
            {
                ElicaseInspectorHeaderRenderer.ApplyToInspector(root);
                Assert.That(title.text, Is.EqualTo("中文组件"));

                Assert.That(ElicaseInspectorComponentTitleProviders.Unregister(provider), Is.True);
                ElicaseInspectorHeaderRenderer.ApplyToInspector(root);
                Assert.That(title.text, Is.EqualTo("Source Component"));
            }
            finally
            {
                ElicaseInspectorComponentTitleProviders.Unregister(provider);
                ElicaseInspectorHeaderRenderer.Restore(root);
            }
        }

        [UnityTest]
        public IEnumerator InspectorWindowAppliesTheTranslatedHeaderTitle()
        {
            var provider = new TextProvider(
                "test.inspector-title.window",
                "Inspector Title Test Component",
                "真实窗口中文标题");
            var previousSelection = Selection.objects;
            var gameObject = new GameObject("Inspector Window Title Test");
            var component = gameObject.AddComponent<InspectorTitleTestComponent>();
            var inspectorWindowType = typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.InspectorWindow");
            var window = EditorWindow.GetWindow(inspectorWindowType);

            ElicaseInspectorComponentTitleProviders.Register(provider);
            try
            {
                Selection.activeObject = component;
                window.Repaint();
                yield return null;
                yield return null;

                ElicaseInspectorHeaderRenderer.ApplyToInspector(window.rootVisualElement);
                Assert.That(
                    provider.MatchedTextTitleRequestCount,
                    Is.GreaterThan(0),
                    "Inspector tree:\n" + DescribeTree(window.rootVisualElement, 0));
            }
            finally
            {
                ElicaseInspectorComponentTitleProviders.Unregister(provider);
                ElicaseInspectorHeaderRenderer.Restore(window.rootVisualElement);
                Selection.objects = previousSelection;
                UnityEngine.Object.DestroyImmediate(gameObject);
            }
        }

        private sealed class TestProvider : IElicaseInspectorComponentTitleProvider
        {
            private readonly bool hasTitle;
            private readonly string title;

            internal TestProvider(string id, bool hasTitle, string title)
            {
                Id = id;
                this.hasTitle = hasTitle;
                this.title = title;
            }

            public string Id { get; }

            public bool TryGetTitle(Component component, out string displayTitle)
            {
                displayTitle = title;
                return hasTitle;
            }
        }

        private static string DescribeTree(VisualElement element, int depth)
        {
            if (depth > 6)
            {
                return string.Empty;
            }

            var text = element as TextElement;
            var description = new string(' ', depth * 2)
                + element.GetType().FullName
                + " name=" + element.name
                + " classes=" + string.Join(",", element.GetClasses())
                + (text == null ? string.Empty : " text=" + text.text)
                + "\n";
            foreach (var child in element.Children())
            {
                description += DescribeTree(child, depth + 1);
            }

            return description;
        }

        private sealed class TextProvider :
            IElicaseInspectorComponentTitleProvider,
            IElicaseInspectorTitleTextProvider
        {
            private readonly string sourceTitle;
            private readonly string translatedTitle;

            internal TextProvider(string id, string sourceTitle, string translatedTitle)
            {
                Id = id;
                this.sourceTitle = sourceTitle;
                this.translatedTitle = translatedTitle;
            }

            public string Id { get; }
            public int ComponentTitleRequestCount { get; private set; }
            public int TextTitleRequestCount { get; private set; }
            public int MatchedTextTitleRequestCount { get; private set; }

            public bool TryGetTitle(Component component, out string title)
            {
                ComponentTitleRequestCount++;
                title = component is InspectorTitleTestComponent ? translatedTitle : null;
                return title != null;
            }

            public bool TryGetTitle(string title, out string displayTitle)
            {
                TextTitleRequestCount++;
                displayTitle = title == sourceTitle ? translatedTitle : null;
                if (displayTitle != null)
                {
                    MatchedTextTitleRequestCount++;
                }

                return displayTitle != null;
            }
        }
    }

    public sealed class InspectorTitleTestComponent : MonoBehaviour
    {
    }
}
