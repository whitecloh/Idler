#if UNITY_EDITOR
using Game.Data.Localization;
using UnityEditor;
using UnityEngine;

namespace Editor_Custom
{
    [CustomPropertyDrawer(typeof(LocalizationListPopupFieldAttribute))]
    public class LocalizationListPopupDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var localizationData = ScriptableObjectAutoInjector.GetInstance<LocalizationData>();
            var options = localizationData != null ? localizationData.GetAllKeys().ToArray() : new[] { property.stringValue };

            var selected = Mathf.Max(0, System.Array.IndexOf(options, property.stringValue));
            var newSelected = EditorGUI.Popup(position, label.text, selected, options);

            if (newSelected >= 0 && newSelected < options.Length)
                property.stringValue = options[newSelected];
        }
    }
}
#endif