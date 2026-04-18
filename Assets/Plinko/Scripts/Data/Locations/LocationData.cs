using System.Collections.Generic;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Meta;
using Plinko.Scripts.Data.Pins;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plinko.Scripts.Data.Locations
{
    [CreateAssetMenu(menuName = "Session/Location", fileName = "LocationData")]
    public sealed class LocationData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [FormerlySerializedAs("art")]
        [SerializeField] private Sprite headerArt;
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private PlinkoFieldSettingsData defaultPlinkoField;
        [SerializeField] private UnlockConditionData unlockCondition;
        [SerializeField] private List<LevelData> levels = new();

        public string Id => id;
        public string DisplayName => displayName;
        public Sprite HeaderArt => headerArt;
        public Sprite Art => headerArt;
        public Sprite BackgroundSprite => backgroundSprite;
        public PlinkoFieldSettingsData DefaultPlinkoField => defaultPlinkoField;
        public UnlockConditionData UnlockCondition => unlockCondition;
        public IReadOnlyList<LevelData> Levels => levels;
    }
}
