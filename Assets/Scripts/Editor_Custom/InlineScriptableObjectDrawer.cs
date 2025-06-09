#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Editor_Custom
{
    [CustomPropertyDrawer(typeof(InlineScriptableObjectAttribute))]
    public class InlineScriptableObjectDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var foldoutWidth = 18f;
            var foldoutRect = new Rect(position.x, position.y, foldoutWidth, EditorGUIUtility.singleLineHeight);
            var fieldRect = new Rect(position.x + foldoutWidth, position.y, position.width - foldoutWidth, EditorGUIUtility.singleLineHeight);

            var foldoutKey = $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}_foldout";
            var expanded = SessionState.GetBool(foldoutKey, false);

            expanded = EditorGUI.Foldout(foldoutRect, expanded, GUIContent.none, true);
            SessionState.SetBool(foldoutKey, expanded);

            EditorGUI.PropertyField(fieldRect, property, label);
            
            if (expanded && property.objectReferenceValue != null)
            {
                EditorGUI.indentLevel++;
                var editor = Editor.CreateEditor(property.objectReferenceValue);
                if (editor != null)
                {
                    Rect boxRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, EditorGUIUtility.currentViewWidth - position.x - 40, GetInspectorHeight(editor));
                    GUILayout.BeginArea(boxRect, GUI.skin.box);
                    editor.OnInspectorGUI();
                    GUILayout.EndArea();
                }
                EditorGUI.indentLevel--;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            var foldoutKey = $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}_foldout";
            var expanded = SessionState.GetBool(foldoutKey, false);

            if (expanded && property.objectReferenceValue != null)
            {
                height += 2 + GetInspectorHeight(Editor.CreateEditor(property.objectReferenceValue));
            }

            return height;
        }

        private float GetInspectorHeight(Editor editor)
        {
            return editor != null ? 80f : 0f;
        }
    }
}
#endif