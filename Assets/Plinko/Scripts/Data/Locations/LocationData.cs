using System.Collections.Generic;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Meta;
using Plinko.Scripts.Data.Pins;
using UnityEngine;

namespace Plinko.Scripts.Data.Locations
{
    [CreateAssetMenu(menuName = "Session/Location", fileName = "LocationData")]
    public sealed class LocationData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private Sprite art;
        [SerializeField] private PlinkoFieldSettingsData defaultPlinkoField;
        [SerializeField] private UnlockConditionData unlockCondition;
        [SerializeField] private List<LevelData> levels = new();

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite Art => art;
        public PlinkoFieldSettingsData DefaultPlinkoField => defaultPlinkoField;
        public UnlockConditionData UnlockCondition => unlockCondition;
        public IReadOnlyList<LevelData> Levels => levels;
    }
}