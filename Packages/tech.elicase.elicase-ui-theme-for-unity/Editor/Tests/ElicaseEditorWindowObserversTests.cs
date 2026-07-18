using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor.Tests
{
    public sealed class ElicaseEditorWindowObserversTests
    {
        [Test]
        public void Register_RejectsDuplicateObserverIds()
        {
            var first = new TestObserver("test.observer.duplicate");
            var duplicate = new TestObserver("test.observer.duplicate");

            ElicaseEditorWindowObservers.Register(first);
            try
            {
                Assert.Throws<ArgumentException>(() => ElicaseEditorWindowObservers.Register(duplicate));
            }
            finally
            {
                ElicaseEditorWindowObservers.Unregister(first);
            }
        }

        [Test]
        public void InspectorElementLookup_LeavesEmptyRootsUntouched()
        {
            Button button;
            Assert.That(ElicaseInspectorElements.TryGetAddComponentButton(new VisualElement(), out button), Is.False);
            Assert.That(button, Is.Null);
        }

        private sealed class TestObserver : IElicaseEditorWindowObserver
        {
            private static readonly IReadOnlyList<ElicaseEditorWindowKind> Inspector =
                new[] { ElicaseEditorWindowKind.Inspector };

            public TestObserver(string id)
            {
                Id = id;
            }

            public string Id { get; }
            public IReadOnlyList<ElicaseEditorWindowKind> TargetWindows => Inspector;
            public void OnAttach(ElicaseEditorWindowContext context) { }
            public void OnRefresh(ElicaseEditorWindowContext context) { }
            public void OnDetach(ElicaseEditorWindowContext context) { }
        }
    }
}
