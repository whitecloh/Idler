using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Game.Save
{
    [Serializable]
    public class SaveItems<TKey, TData> : IEnumerable<KeyValuePair<TKey, TData>> where TData : new()
    {
        [Serializable]
        public class Items : SerializedDictionary<TKey, TData>
        {
            public Items() { }
            public Items(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
        
        [SerializeField]
        private Items items = new ();

        public TData this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public TData Get(TKey item)
        {
            if (items.TryGetValue(item, out var value))
                return value;

            value = new TData();
            items.Add(item, value);

            return value;
        }

        public void Set(TKey item, TData data)
        {
            items[item] = data;
        }

        public void Remove(TKey item)
        {
            items.Remove(item);
        }

        public bool ContainsKey(TKey item)
        {
            return items.ContainsKey(item);
        }

        public void Clear() => items.Clear();

        public IEnumerator<KeyValuePair<TKey, TData>> GetEnumerator() => items.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}