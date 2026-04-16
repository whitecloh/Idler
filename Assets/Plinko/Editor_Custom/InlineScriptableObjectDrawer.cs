using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

namespace Plinko.Editor_Custom
{
    [CustomPropertyDrawer(typeof(InlineScriptableObjectAttribute))]
    public sealed class InlineScriptableObjectDrawer : PropertyDrawer
    {
        private const float FoldoutWidth = 18f;
        private const float InspectorHeight = 80f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var foldoutRect = new Rect(position.x, position.y, FoldoutWidth, EditorGUIUtility.singleLineHeight);
            var fieldRect = new Rect(position.x + FoldoutWidth, position.y, position.width - FoldoutWidth,
                EditorGUIUtility.singleLineHeight);

            var foldoutKey = $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}_foldout";
            var expanded = SessionState.GetBool(foldoutKey, false);

            expanded = EditorGUI.Foldout(foldoutRect, expanded, GUIContent.none, true);
            SessionState.SetBool(foldoutKey, expanded);

            EditorGUI.PropertyField(fieldRect, property, label);

            if (!expanded || property.objectReferenceValue == null)
                return;

            EditorGUI.indentLevel++;
            var editor = Editor.CreateEditor(property.objectReferenceValue);
            if (editor != null)
            {
                var boxRect = new Rect(
                    position.x,
                    position.y + EditorGUIUtility.singleLineHeight + 2f,
                    EditorGUIUtility.currentViewWidth - position.x - 40f,
                    InspectorHeight);

                GUILayout.BeginArea(boxRect, GUI.skin.box);
                editor.OnInspectorGUI();
                GUILayout.EndArea();
            }
            EditorGUI.indentLevel--;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var height = EditorGUIUtility.singleLineHeight;

            var foldoutKey = $"{property.serializedObject.targetObject.GetInstanceID()}_{property.propertyPath}_foldout";
            var expanded = SessionState.GetBool(foldoutKey, false);

            if (expanded && property.objectReferenceValue != null)
                height += 2f + InspectorHeight;

            return height;
        }
    }
}
#endif