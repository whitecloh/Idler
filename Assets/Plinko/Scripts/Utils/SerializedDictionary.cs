namespace Utils
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using UnityEngine;
    
    [Serializable]
    public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [HideInInspector] [SerializeField] private List<TKey> keyData = new();

        [HideInInspector] [SerializeField] private List<TValue> valueData = new();

        public SerializedDictionary() { }

        public SerializedDictionary(SerializationInfo info, StreamingContext context) : base(info, context) { }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Clear();

            for (var i = 0; i < keyData.Count && i < valueData.Count; i++)
            {
                this[keyData[i]] = valueData[i];
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            keyData.Clear();
            valueData.Clear();

            foreach (var item in this)
            {
                keyData.Add(item.Key);
                valueData.Add(item.Value);
            }
        }
    }
}