using System.Collections.Generic;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Enemies;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Stats;
using Plinko.Scripts.Data.Units;
using Plinko.Scripts.Models.ViewData;

namespace Plinko.Scripts.Services
{
    public static class StatViewDataFactory
    {
        public static List<StatDisplayViewData> BuildUnitStats(
            StatTypeConfigService statTypeConfigService,
            UnitTypeData unitType)
        {
            if (unitType == null || unitType.BaseStats == null || unitType.BaseStats.Count <= 0)
            {
                return new List<StatDisplayViewData>();
            }

            var result = new List<StatDisplayViewData>(unitType.BaseStats.Count);
            for (var index = 0; index < unitType.BaseStats.Count; index++)
            {
                var entry = unitType.BaseStats[index];
                if (entry == null || entry.StatTypeId == StatTypeIds.ManaCost)
                {
                    continue;
                }

                result.Add(Build(
                    statTypeConfigService,
                    entry.StatType,
                    entry.StatTypeId,
                    entry.Value,
                    signed: false));
            }

            return result;
        }

        public static List<StatDisplayViewData> BuildUnitStats(
            StatTypeConfigService statTypeConfigService,
            UnitTypeData unitType,
            int attack,
            int health,
            int manaCost,
            float moveSpeed,
            int attackRange,
            float attackSpeed)
        {
            if (unitType != null && unitType.BaseStats != null && unitType.BaseStats.Count > 0)
            {
                var result = new List<StatDisplayViewData>(unitType.BaseStats.Count);
                for (var index = 0; index < unitType.BaseStats.Count; index++)
                {
                    var entry = unitType.BaseStats[index];
                    if (entry == null || entry.StatTypeId == StatTypeIds.ManaCost)
                    {
                        continue;
                    }

                    var currentValue = ResolveCurrentUnitStatValue(
                        unitType,
                        entry.StatTypeId,
                        attack,
                        health,
                        manaCost,
                        moveSpeed,
                        attackRange,
                        attackSpeed);
                    result.Add(Build(statTypeConfigService, entry.StatType, entry.StatTypeId, currentValue, signed: false));
                }

                return result;
            }

            return new List<StatDisplayViewData>();
        }

        public static List<StatDisplayViewData> BuildPinModifierStats(
            StatTypeConfigService statTypeConfigService,
            PinTypeData pinType)
        {
            if (pinType == null || pinType.Modifiers == null || pinType.Modifiers.Count <= 0)
            {
                return new List<StatDisplayViewData>();
            }

            var result = new List<StatDisplayViewData>(pinType.Modifiers.Count);
            for (var index = 0; index < pinType.Modifiers.Count; index++)
            {
                var entry = pinType.Modifiers[index];
                if (entry == null || entry.StatTypeId == StatTypeIds.ManaCost)
                {
                    continue;
                }

                result.Add(Build(
                    statTypeConfigService,
                    entry.StatType,
                    entry.StatTypeId,
                    entry.Value,
                    signed: true));
            }

            return result;
        }

        public static List<StatDisplayViewData> BuildEnemyStats(
            StatTypeConfigService statTypeConfigService,
            EnemyUnitSpawnData enemy)
        {
            return enemy != null && enemy.BaseStats != null && enemy.BaseStats.Count > 0
                ? BuildFromEntries(statTypeConfigService, enemy.BaseStats, signed: false)
                : new List<StatDisplayViewData>();
        }

        private static List<StatDisplayViewData> BuildFromEntries(
            StatTypeConfigService statTypeConfigService,
            IReadOnlyList<StatValueEntryData> entries,
            bool signed)
        {
            var result = new List<StatDisplayViewData>();
            if (entries == null)
            {
                return result;
            }

            for (var index = 0; index < entries.Count; index++)
            {
                var entry = entries[index];
                if (entry == null)
                {
                    continue;
                }

                result.Add(Build(
                    statTypeConfigService,
                    entry.StatType,
                    entry.StatTypeId,
                    entry.Value,
                    signed));
            }

            return result;
        }

        private static float ResolveCurrentUnitStatValue(
            UnitTypeData unitType,
            string statTypeId,
            int attack,
            int health,
            int manaCost,
            float moveSpeed,
            int attackRange,
            float attackSpeed)
        {
            return statTypeId switch
            {
                StatTypeIds.Attack => attack,
                StatTypeIds.Health => health,
                StatTypeIds.MoveRange => unitType != null ? unitType.BattleMoveRange : 0,
                StatTypeIds.MoveSpeed => moveSpeed,
                StatTypeIds.AttackRange => attackRange,
                StatTypeIds.AttackSpeed => attackSpeed,
                StatTypeIds.PassiveIncome => unitType != null ? unitType.PassiveIncomePerTick : 0,
                _ => unitType != null && unitType.TryGetBaseStatValue(statTypeId, out var value) ? value : 0f
            };
        }

        private static StatDisplayViewData BuildInt(
            StatTypeConfigService statTypeConfigService,
            string statTypeId,
            int value)
        {
            return Build(statTypeConfigService, null, statTypeId, value, signed: false);
        }

        private static StatDisplayViewData BuildSignedInt(
            StatTypeConfigService statTypeConfigService,
            string statTypeId,
            int value)
        {
            return Build(statTypeConfigService, null, statTypeId, value, signed: true);
        }

        private static StatDisplayViewData BuildFloat(
            StatTypeConfigService statTypeConfigService,
            string statTypeId,
            float value)
        {
            return Build(statTypeConfigService, null, statTypeId, value, signed: false);
        }

        private static StatDisplayViewData BuildSignedFloat(
            StatTypeConfigService statTypeConfigService,
            string statTypeId,
            float value)
        {
            return Build(statTypeConfigService, null, statTypeId, value, signed: true);
        }

        private static StatDisplayViewData Build(
            StatTypeConfigService statTypeConfigService,
            StatTypeData statType,
            string statTypeId,
            float value,
            bool signed)
        {
            var resolvedStatType = statType != null
                ? statType
                : statTypeConfigService != null
                    ? statTypeConfigService.GetStat(statTypeId)
                    : null;
            return new StatDisplayViewData
            {
                StatTypeId = statTypeId,
                DisplayName = resolvedStatType != null && !string.IsNullOrWhiteSpace(resolvedStatType.DisplayName)
                    ? resolvedStatType.DisplayName
                    : GetFallbackDisplayName(statTypeId),
                Icon = resolvedStatType != null ? resolvedStatType.Icon : null,
                ValueText = FormatValue(statTypeId, value, signed)
            };
        }

        private static string FormatValue(string statTypeId, float value, bool signed)
        {
            var showAsInteger = IsIntegerStat(statTypeId);
            if (showAsInteger)
            {
                var intValue = UnityEngine.Mathf.RoundToInt(value);
                return signed && intValue > 0 ? $"+{intValue}" : intValue.ToString();
            }

            var floatText = value.ToString("0.##");
            return signed && value > 0f ? $"+{floatText}" : floatText;
        }

        private static bool IsIntegerStat(string statTypeId)
        {
            return statTypeId == StatTypeIds.Attack ||
                   statTypeId == StatTypeIds.Health ||
                   statTypeId == StatTypeIds.MoveRange ||
                   statTypeId == StatTypeIds.ManaCost ||
                   statTypeId == StatTypeIds.AttackRange ||
                   statTypeId == StatTypeIds.PassiveIncome;
        }

        private static string GetFallbackDisplayName(string statTypeId)
        {
            return statTypeId switch
            {
                StatTypeIds.Attack => "ATK",
                StatTypeIds.Health => "HP",
                StatTypeIds.MoveRange => "Move",
                StatTypeIds.ManaCost => "Mana",
                StatTypeIds.MoveSpeed => "Move",
                StatTypeIds.AttackRange => "Range",
                StatTypeIds.AttackSpeed => "ASPD",
                StatTypeIds.PassiveIncome => "Income",
                _ => statTypeId
            };
        }
    }
}
