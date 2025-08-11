#if UNITY_EDITOR

namespace Editor_Custom
{
    using Game.Data.Business;
    using UnityEditor;
    using UnityEngine;
    
    [CustomEditor(typeof(BusinessesConfigsData))]
    public sealed class BusinessTypeIdSettingsEditor : Editor
    {
        private const float LabelItemNameWidth = 70f;
        private const float TextItemNameWidth = 180f;
        private const float LabelBusinessIdWidth = 80f;
        private const float LabelDataWidth = 40f;
        private const float ObjectFieldWidth = 220f;
        private const float RemoveButtonWidth = 80f;

        private BusinessId _newId = BusinessId.None;
        private string _newName = string.Empty;

        public override void OnInspectorGUI()
        {
            var idSettings = (BusinessesConfigsData)target;

            EditorGUILayout.LabelField("Business List", EditorStyles.boldLabel);

            var toRemove = -1;
            for (var i = 0; i < idSettings.Items.Count; i++)
            {
                var item = idSettings.Items[i];
                EditorGUILayout.BeginHorizontal();
                
                EditorGUILayout.LabelField("ItemName", GUILayout.Width(LabelItemNameWidth));
                EditorGUI.BeginChangeCheck();
                var newName = EditorGUILayout.TextField(item.ItemName, GUILayout.Width(TextItemNameWidth));
                if (EditorGUI.EndChangeCheck() && newName != item.ItemName)
                {
                    Undo.RecordObject(idSettings, "Rename Business Item");
                    item.SetName(newName);
                    EditorUtility.SetDirty(idSettings);
                }
                
                EditorGUILayout.LabelField("BusinessId", GUILayout.Width(LabelBusinessIdWidth));
                EditorGUILayout.LabelField($"{item.Id} ({(int)item.Id})", GUILayout.Width(LabelBusinessIdWidth + 30f));
                
                EditorGUILayout.LabelField("Data", GUILayout.Width(LabelDataWidth));
                var newData = (BusinessConfigData)EditorGUILayout.ObjectField(
                    item.Data, typeof(BusinessConfigData), false, GUILayout.Width(ObjectFieldWidth));

                if (newData != item.Data)
                {
                    Undo.RecordObject(idSettings, "Assign BusinessConfigData");
                    item.SetData(newData);
                    EditorUtility.SetDirty(idSettings);
                }

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Remove", GUILayout.Width(RemoveButtonWidth)))
                    toRemove = i;

                EditorGUILayout.EndHorizontal();
            }

            if (toRemove >= 0)
            {
                idSettings.RemoveAt(toRemove);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Add New Business", EditorStyles.boldLabel);

            _newName = EditorGUILayout.TextField("ItemName", _newName);
            _newId = (BusinessId)EditorGUILayout.EnumPopup("BusinessId", _newId);

            var isNameValid = !string.IsNullOrWhiteSpace(_newName);
            var isIdValid = _newId != BusinessId.None;

            using (new EditorGUI.DisabledScope(!(isNameValid && isIdValid)))
            {
                if (GUILayout.Button("Add Business"))
                {
                    Undo.RecordObject(idSettings, "Add Business");
                    idSettings.AddItem(_newName.Trim(), _newId);
                    EditorUtility.SetDirty(idSettings);

                    _newName = string.Empty;
                    _newId = BusinessId.None;
                }
            }
        }
    }
}
#endif