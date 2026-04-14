using System.Collections.Generic;
using Plinko.Scripts.Data.Units;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class UnitConfigService
    {
        private readonly Dictionary<string, UnitTypeData> _unitsById = new();
        private readonly List<UnitTypeData> _allUnits = new();

        public UnitConfigService(IReadOnlyList<UnitTypeData> unitTypes)
        {
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
        
        public IReadOnlyList<UnitTypeData> GetShopUnits(HashSet<string> excludedUnitTypeIds)
        {
            var result = new List<UnitTypeData>();
            foreach (var unit in _allUnits)
            {
                if (unit == null)
                {
                    continue;
                }

                if (excludedUnitTypeIds != null && excludedUnitTypeIds.Contains(unit.Id))
                {
                    continue;
                }

                result.Add(unit);
            }

            return result;
        }
        
        public UnitTypeData GetNextShopUnit(string currentUnitTypeId, HashSet<string> excludedUnitTypeIds)
        {
            if (_allUnits.Count == 0)
            {
                return null;
            }

            var currentIndex = _allUnits.FindIndex(unit => unit != null && unit.Id == currentUnitTypeId);
            for (var step = 1; step <= _allUnits.Count; step++)
            {
                var index = (Mathf.Max(currentIndex, -1) + step) % _allUnits.Count;
                var candidate = _allUnits[index];
                if (candidate == null)
                {
                    continue;
                }

                if (excludedUnitTypeIds != null && excludedUnitTypeIds.Contains(candidate.Id))
                {
                    continue;
                }

                return candidate;
            }

            return null;
        }

        public IReadOnlyList<UnitTypeData> GetAllUnits()
        {
            return _allUnits;
        }
    }
}