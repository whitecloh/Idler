#if UNITY_EDITOR

namespace Editor_Custom
{
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    
    [CustomPropertyDrawer(typeof(NameKeyPopupAttribute))]
    public class NameKeyPopupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
                return;
            }

            var catalog = NamesCatalogFinder.FindCatalog();
            if (catalog == null)
            {
                EditorGUI.PropertyField(position, property, label);

                var helpPos = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + 2,
                    position.width,
                    EditorGUIUtility.singleLineHeight
                );
                EditorGUI.HelpBox(helpPos,
                    "NamesCatalog не найден. Создайте его: Create > IdleClicker > NamesCatalog.",
                    MessageType.Info);

                EditorGUI.EndProperty();
                return;
            }

            var keys = catalog.GetKeys();
            if (keys == null || keys.Count == 0)
            {
                EditorGUI.PropertyField(position, property, label);
                EditorGUI.EndProperty();
                return;
            }
            
            var current = property.stringValue;
            var index = Mathf.Max(0, keys.ToList().FindIndex(k => k == current));
            
            var options = keys.ToArray();
            var newIndex = EditorGUI.Popup(position, label.text, index, options);
            if (newIndex >= 0 && newIndex < options.Length) property.stringValue = options[newIndex];

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var hasCatalog = NamesCatalogFinder.FindCatalog() != null;
            return hasCatalog
                ? EditorGUIUtility.singleLineHeight
                : EditorGUIUtility.singleLineHeight * 2f + 2f;
        }
    }
}
#endif