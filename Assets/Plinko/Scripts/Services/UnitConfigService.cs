using System.Collections.Generic;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Data.Units;

namespace Plinko.Scripts.Services
{
    public sealed class UnitConfigService
    {
        private readonly Dictionary<string, UnitTypeData> _unitsById = new();
        private readonly List<UnitTypeData> _allUnits = new();
        private readonly UnlocksService _unlocksService;

        public UnitConfigService(IReadOnlyList<UnitTypeData> unitTypes, UnlocksService unlocksService)
        {
            _unlocksService = unlocksService;
            if (unitTypes == null)
            {
                return;
            }

            foreach (var unitType in unitTypes)
            {
                if (unitType == null || string.IsNullOrWhiteSpace(unitType.Id))
                {
                    continue;
                }

                _unitsById[unitType.Id] = unitType;
                _allUnits.Add(unitType);
            }
        }

        public UnitTypeData GetUnit(string unitTypeId)
        {
            return !string.IsNullOrWhiteSpace(unitTypeId) && _unitsById.TryGetValue(unitTypeId, out var unit)
                ? unit
                : null;
        }

        public IReadOnlyList<UnitTypeData> GetUnlockedShopPool(LevelData levelData)
        {
            var result = new List<UnitTypeData>();
            var explicitPool = levelData != null && levelData.PreBattlePhase != null
                ? levelData.PreBattlePhase.ExplicitUnitShopPool
                : null;

            if (explicitPool != null && explicitPool.Count > 0)
            {
                foreach (var unit in explicitPool)
                {
                    if (unit != null && _unlocksService.IsUnlocked(unit.UnlockCondition))
                    {
                        result.Add(unit);
                    }
                }

                return result;
            }

            foreach (var unit in _allUnits)
            {
                if (_unlocksService.IsUnlocked(unit.UnlockCondition))
                {
                    result.Add(unit);
                }
            }

            return result;
        }
    }
}