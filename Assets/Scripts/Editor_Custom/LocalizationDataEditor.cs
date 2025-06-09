#if UNITY_EDITOR
using System.Collections.Generic;
using System.Net;
using Game.Data.Localization;
using UnityEditor;
using UnityEngine;

namespace Editor_Custom
{
    [CustomEditor(typeof(LocalizationData))]
    public class LocalizationDataEditor : Editor
    {
        private string _googleCsvUrl = "https://docs.google.com/spreadsheets/d/1pNK88rvzIDC83P3sSzILoC01t9oy1K4aOZb9tB7Qgso/export?format=csv&gid=0";
        private int _keyCol;
        private int _enCol = 1;
        private int _ruCol = 2;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);
            GUILayout.Label("CSV Import Settings", EditorStyles.boldLabel);
            _googleCsvUrl = EditorGUILayout.TextField("Google Csv Url", _googleCsvUrl);
            _keyCol = EditorGUILayout.IntField("Key Column", _keyCol);
            _enCol = EditorGUILayout.IntField("English Column", _enCol);
            _ruCol = EditorGUILayout.IntField("Russian Column", _ruCol);

            if (GUILayout.Button("Parse from Google Sheets"))
            {
                if (!string.IsNullOrEmpty(_googleCsvUrl))
                {
                    ImportFromGoogleCsv((LocalizationData)target, _googleCsvUrl, _keyCol, _enCol, _ruCol);
                }
            }
        }

        private void ImportFromGoogleCsv(LocalizationData localizationData, string url, int keyColumn, int enColumn, int ruColumn)
        {
            using var client = new WebClient();
            
            try
            {
                var csv = client.DownloadString(url);
                var lines = csv.Split('\n');
                var blocks = new List<LocalizationData.LanguageBlock>();
                    
                for (var i = 1; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    var cols = line.Split(',');
                    if (cols.Length <= Mathf.Max(keyColumn, enColumn, ruColumn))
                        continue;

                    var block = new LocalizationData.LanguageBlock();
                    var key = cols[keyColumn].Trim();
                    if (string.IsNullOrEmpty(key)) continue;
                    typeof(LocalizationData.LanguageBlock).GetField("key", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(block, key);

                    var texts = new List<LanguageTextPair>
                    {
                        new() { language = SystemLanguage.English, text = cols[enColumn].Trim() },
                        new() { language = SystemLanguage.Russian, text = cols[ruColumn].Trim() }
                    };
                    typeof(LocalizationData.LanguageBlock).GetField("texts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                        ?.SetValue(block, texts);

                    blocks.Add(block);
                }
                typeof(LocalizationData).GetField("languageBlocks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.SetValue(localizationData, blocks);

                EditorUtility.SetDirty(localizationData);
                Debug.Log($"LocalizationData imported successfully: {blocks.Count} keys!");
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Error importing CSV: " + ex);
            }
        }
    }
}
#endif