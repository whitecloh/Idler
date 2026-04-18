using System.Collections.Generic;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Units;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plinko.Scripts.Data.Levels
{
    [CreateAssetMenu(menuName = "Session/LevelPhase", fileName = "LevelPhaseData")]
    public sealed class LevelPhaseData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [FormerlySerializedAs("overrideRetrainingSelectionLimit")]
        [SerializeField] private int overrideRetrainingOfferCount;
        [SerializeField] private List<UnitTypeData> explicitUnitShopPool = new();
        [SerializeField] private List<PinTypeData> explicitPinShopPool = new();
        [SerializeField] private PlinkoFieldSettingsData overridePlinkoField;

        public string Id => id;
        public int OverrideRetrainingOfferCount => overrideRetrainingOfferCount;
        public int OverrideRetrainingSelectionLimit => overrideRetrainingOfferCount;
        public IReadOnlyList<UnitTypeData> ExplicitUnitShopPool => explicitUnitShopPool;
        public IReadOnlyList<PinTypeData> ExplicitPinShopPool => explicitPinShopPool;
        public PlinkoFieldSettingsData OverridePlinkoField => overridePlinkoField;
    }
}
