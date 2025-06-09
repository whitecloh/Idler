using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.Data.Localization
{
    [Serializable]
    public class LanguageTextPair
    {
        public SystemLanguage language;
        public string text;
    }

    [CreateAssetMenu(menuName = "IdleClicker/LocalizationData", fileName = "LocalizationData")]
    public class LocalizationData : ScriptableObject
    {
        [Serializable]
        public class LanguageBlock
        {
            [SerializeField] private string key;
            [SerializeField] private List<LanguageTextPair> texts = new();

            public string Key => key;

            public string GetText(SystemLanguage language)
            {
                var pair = texts.Find(t => t.language == language);
                return pair != null ? pair.text : key;
            }

            public IReadOnlyList<LanguageTextPair> Texts => texts;
        }

        [SerializeField] private List<LanguageBlock> languageBlocks = new();

        public IReadOnlyList<LanguageBlock> LanguageBlocks => languageBlocks;
        
#if UNITY_EDITOR
        public List<string> GetAllKeys()
        {
            return languageBlocks.Select(block => block.Key).ToList();
        }
#endif
        public bool TryGetText(string key, SystemLanguage language, out string text)
        {
            foreach (var block in languageBlocks)
            {
                if (block.Key != key) 
                    continue;
                
                var pair = block.Texts.FirstOrDefault(t => t.language == language && !string.IsNullOrEmpty(t.text));
                if (pair != null)
                {
                    text = pair.text;
                    return true;
                }
                    
                var anyPair = block.Texts.FirstOrDefault(t => !string.IsNullOrEmpty(t.text));
                if (anyPair != null)
                {
                    text = anyPair.text;
                    return true;
                }
                    
                break;
            }
            
            text = key;
            return false;
        }
    }
}