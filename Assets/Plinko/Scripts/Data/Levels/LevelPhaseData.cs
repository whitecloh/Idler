using System.Collections.Generic;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Units;
using UnityEngine;

namespace Plinko.Scripts.Data.Levels
{
    [CreateAssetMenu(menuName = "Session/LevelPhase", fileName = "LevelPhaseData")]
    public sealed class LevelPhaseData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private int overrideRetrainingSelectionLimit;
        [SerializeField] private List<UnitTypeData> explicitUnitShopPool = new();
        [SerializeField] private List<PinTypeData> explicitPinShopPool = new();
        [SerializeField] private PlinkoFieldSettingsData overridePlinkoField;

        public string Id => id;
        public int OverrideRetrainingSelectionLimit => overrideRetrainingSelectionLimit;
        public IReadOnlyList<UnitTypeData> ExplicitUnitShopPool => explicitUnitShopPool;
        public IReadOnlyList<PinTypeData> ExplicitPinShopPool => explicitPinShopPool;
        public PlinkoFieldSettingsData OverridePlinkoField => overridePlinkoField;
    }
}