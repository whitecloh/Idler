namespace Game.Data.Names
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    
    [CreateAssetMenu(menuName = "IdleClicker/NamesCatalog", fileName = "NamesCatalog")]
    public class NamesCatalog : ScriptableObject
    {
        [SerializeField] private List<NameItem> items = new();

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            foreach (var t in items.Where(t => t.Key == key))
                return string.IsNullOrEmpty(t.Text) ? key : t.Text;

            return key;
        }

        public IReadOnlyList<string> GetKeys()
        {
            var set = new HashSet<string>();
            var list = (from item in items where !string.IsNullOrEmpty(item.Key) && set.Add(item.Key) select item.Key).ToList();
            if (list.Count == 0)
            {
                list.Add(string.Empty);
            }
            
            return list;
        }

        [Serializable]
        public struct NameItem
        {
            [SerializeField] private string key;
            [TextArea] 
            [SerializeField] private string text;

            public string Key => key;
            public string Text => text;
        }
    }
}