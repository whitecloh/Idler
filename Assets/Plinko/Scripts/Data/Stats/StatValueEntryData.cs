using System;
using UnityEngine;

namespace Plinko.Scripts.Data.Stats
{
    [Serializable]
    public sealed class StatValueEntryData
    {
        [SerializeField] private StatTypeData statType;
        [SerializeField] private float value;

        public StatTypeData StatType => statType;
        public string StatTypeId => statType != null ? statType.Id : string.Empty;
        public float Value => value;
    }
}
