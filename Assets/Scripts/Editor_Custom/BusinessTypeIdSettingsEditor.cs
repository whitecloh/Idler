#if UNITY_EDITOR
using Game.Data.Business;
using UnityEditor;
using UnityEngine;

namespace Editor_Custom
{
    [CustomEditor(typeof(BusinessesConfigsData))]
    public class BusinessTypeIdSettingsEditor : Editor
    {
        private string _newName = string.Empty;
        private BusinessId _newId = BusinessId.None;

        public override void OnInspectorGUI()
        {
            var idSettings = (BusinessesConfigsData)target;

            EditorGUILayout.LabelField("Business List", EditorStyles.boldLabel);

            int toRemove = -1;
            for (var i = 0; i < idSettings.Items.Count; i++)
            {
                var item = idSettings.Items[i];
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("ItemName", GUILayout.Width(45));
                EditorGUILayout.TextField(item.ItemName, GUILayout.Width(150));
                EditorGUILayout.LabelField("BusinessId", GUILayout.Width(70));
                EditorGUILayout.LabelField($"{item.Id} ({(int)item.Id})", GUILayout.Width(110));
                EditorGUILayout.LabelField("Data", GUILayout.Width(40));

                var newData = (BusinessConfigData)EditorGUILayout.ObjectField(item.Data, typeof(BusinessConfigData), false, GUILayout.Width(200));
#if UNITY_EDITOR
                if (newData != item.Data)
                {
                    Undo.RecordObject(idSettings, "Assign BusinessConfigData");
                    item.SetData(newData);
                    EditorUtility.SetDirty(idSettings);
                }
#endif

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Remove", GUILayout.Width(80)))
                {
                    toRemove = i;
                }

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

            GUI.enabled = !string.IsNullOrWhiteSpace(_newName);

            if (GUILayout.Button("Add Business"))
            {
                idSettings.AddItem(_newName.Trim(), _newId);
                _newName = string.Empty;
                _newId = BusinessId.None;
            }
            GUI.enabled = true;
        }
    }
}
#endif