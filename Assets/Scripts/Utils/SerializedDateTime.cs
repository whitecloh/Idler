using System;
using UnityEngine;

namespace Utils
{
    [Serializable]
    public class SerializedDateTime : ISerializationCallbackReceiver
    { 
        [SerializeField]
        private string dateTimeString;

        [NonSerialized]
        private DateTime _value;

        public SerializedDateTime() : this(DateTime.MinValue) { }
        public SerializedDateTime(DateTime dateTime)
        {
            _value = dateTime.ToUniversalTime();
            dateTimeString = _value.ToString("o"); // ISO 8601 format
        }

        public DateTime Value
        {
            get => _value;
            set
            {
                _value = value.ToUniversalTime();
                dateTimeString = _value.ToString("o");
            }
        }

        public static implicit operator DateTime(SerializedDateTime sdt) => sdt._value;
        public static implicit operator SerializedDateTime(DateTime dt) => new SerializedDateTime(dt);

        public void OnBeforeSerialize()
        {
            dateTimeString = _value.ToString("o");
        }

        public void OnAfterDeserialize()
        {
            if (!string.IsNullOrEmpty(dateTimeString) && DateTime.TryParse(dateTimeString, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsed))
            {
                _value = parsed.ToUniversalTime();
            }
            else
            {
                _value = DateTime.MinValue;
            }
        }

        public override string ToString() => _value.ToString("o");
    }
}