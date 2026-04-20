using Plinko.Scripts.Data.Stats;
using UnityEditor;
using UnityEngine;

namespace Plinko.Scripts.Editor
{
    [CustomPropertyDrawer(typeof(StatValueEntryData))]
    public sealed class StatValueEntryDataDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var statTypeProperty = property.FindPropertyRelative("statType");
            var valueProperty = property.FindPropertyRelative("value");

            var labelRect = new Rect(position.x, position.y, EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            var contentRect = new Rect(position.x + EditorGUIUtility.labelWidth, position.y, position.width - EditorGUIUtility.labelWidth, EditorGUIUtility.singleLineHeight);
            var typeWidth = Mathf.Max(120f, contentRect.width * 0.65f);
            var typeRect = new Rect(contentRect.x, contentRect.y, typeWidth, contentRect.height);
            var valueRect = new Rect(typeRect.xMax + 4f, contentRect.y, Mathf.Max(60f, contentRect.width - typeWidth - 4f), contentRect.height);

            EditorGUI.PrefixLabel(labelRect, label);
            EditorGUI.PropertyField(typeRect, statTypeProperty, GUIContent.none);
            EditorGUI.PropertyField(valueRect, valueProperty, GUIContent.none);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}
