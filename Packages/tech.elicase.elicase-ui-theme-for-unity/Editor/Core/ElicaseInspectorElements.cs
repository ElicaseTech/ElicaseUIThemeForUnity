using System;
using System.Reflection;
using UnityEditor;
using UnityEngine.UIElements;

namespace Tech.Elicase.UITheme.Editor
{
    public static class ElicaseInspectorElements
    {
        private const string InspectorElementTypeName = "UnityEditor.InspectorElement";
        private const string AddComponentClassNameField = "s_AddComponentClassName";

        private static readonly Type InspectorElementType =
            typeof(UnityEditor.Editor).Assembly.GetType(InspectorElementTypeName);

        private static readonly FieldInfo AddComponentClassField = InspectorElementType == null
            ? null
            : InspectorElementType.GetField(
                AddComponentClassNameField,
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

        public static bool TryGetAddComponentButton(VisualElement root, out Button button)
        {
            button = null;
            if (root == null || AddComponentClassField == null)
            {
                return false;
            }

            var className = AddComponentClassField.GetValue(null) as string;
            if (string.IsNullOrEmpty(className))
            {
                return false;
            }

            button = root.Q<Button>(className: className);
            return button != null;
        }
    }
}
