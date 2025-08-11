namespace Game.Save
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using UnityEngine;
    using Utils;
    
    [Serializable]
    public class SaveItems<TKey, TData> : IEnumerable<KeyValuePair<TKey, TData>> where TData : new()
    {
        [SerializeField] private Items items = new();

        public TData this[TKey key]
        {
            get => Get(key);
            set => Set(key, value);
        }

        public IEnumerator<KeyValuePair<TKey, TData>> GetEnumerator()
        {
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
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

        public void Clear()
        {
            items.Clear();
        }

        [Serializable]
        public class Items : SerializedDictionary<TKey, TData>
        {
            public Items() { }

            public Items(SerializationInfo info, StreamingContext context) : base(info, context) { }
        }
    }
}