using System.Collections.Generic;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Units;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class PlinkoPathFactory
    {
        public PlinkoPathResultModel GeneratePurchaseResult(
            int runtimeId,
            UnitTypeData unitType,
            string displayName,
            PlinkoFieldSettingsData field,
            IReadOnlyDictionary<int, PinTypeData> installedPins)
        {
            return GenerateResult(
                runtimeId,
                unitType.Id,
                displayName,
                unitType.BaseAttack,
                unitType.BaseHealth,
                unitType.DefaultManaCost,
                unitType.PassiveAbility.Id,
                1,
                0,
                field,
                installedPins);
        }

        public PlinkoPathResultModel GenerateRetrainingResult(
            int runtimeId,
            string unitTypeId,
            string displayName,
            int attack,
            int health,
            int manaCost,
            string passiveAbilityId,
            int level,
            int upgradeCount,
            PlinkoFieldSettingsData field,
            IReadOnlyDictionary<int, PinTypeData> installedPins)
        {
            return GenerateResult(runtimeId, unitTypeId, displayName, attack, health, manaCost, passiveAbilityId, level, upgradeCount, field, installedPins);
        }

        private PlinkoPathResultModel GenerateResult(
            int runtimeId,
            string unitTypeId,
            string displayName,
            int baseAttack,
            int baseHealth,
            int baseManaCost,
            string passiveAbilityId,
            int level,
            int upgradeCount,
            PlinkoFieldSettingsData field,
            IReadOnlyDictionary<int, PinTypeData> installedPins)
        {
            var result = new PlinkoPathResultModel
            {
                Nodes = new List<PlinkoPathNodeModel>()
            };

            var currentColumn = 0;
            var currentAttack = baseAttack;
            var currentHealth = baseHealth;
            var slotIndex = 0;

            if (field != null && field.Rows != null)
            {
                for (var rowIndex = 0; rowIndex < field.Rows.Count; rowIndex++)
                {
                    var row = field.Rows[rowIndex];
                    var rowCount = row != null && row.Cells != null ? row.Cells.Count : 0;
                    if (rowCount <= 0)
                    {
                        continue;
                    }

                    currentColumn = rowIndex == 0
                        ? Mathf.Clamp(currentColumn, 0, rowCount - 1)
                        : Mathf.Clamp(currentColumn + UnityEngine.Random.Range(0, 2), 0, rowCount - 1);

                    var currentSlot = slotIndex + currentColumn;
                    installedPins.TryGetValue(currentSlot, out var runtimePin);
                    var authoredPin = runtimePin;
                    if (authoredPin == null && row.Cells[currentColumn] != null)
                    {
                        authoredPin = row.Cells[currentColumn].PinType;
                    }

                    var node = new PlinkoPathNodeModel
                    {
                        RowIndex = rowIndex,
                        ColumnIndex = currentColumn,
                        PinTypeId = authoredPin != null ? authoredPin.Id : string.Empty,
                        AttackDelta = authoredPin != null ? authoredPin.AttackModifier : 0,
                        HealthDelta = authoredPin != null ? authoredPin.HealthModifier : 0,
                        ManaDelta = authoredPin != null ? authoredPin.ManaModifier : 0
                    };

                    currentAttack += node.AttackDelta;
                    currentHealth += node.HealthDelta;
                    result.Nodes.Add(node);
                    slotIndex += rowCount;
                }
            }
            
            var finalMana = Mathf.Max(1, baseManaCost);
            var finalBasketId = string.Empty;
            if (field != null && field.Baskets != null && field.Baskets.Count > 0)
            {
                var firstIndex = Mathf.Clamp(currentColumn, 0, field.Baskets.Count - 1);
                var secondIndex = Mathf.Clamp(firstIndex + 1, 0, field.Baskets.Count - 1);
                var basket = UnityEngine.Random.value < 0.5f ? field.Baskets[firstIndex] : field.Baskets[secondIndex];
                finalMana = basket != null ? basket.ManaValue : finalMana;
                finalBasketId = basket != null ? basket.Id : string.Empty;
            }

            result.FinalBasketId = finalBasketId;
            result.FinalBasketManaValue = finalMana;
            result.Result = new TrainedUnitResultModel
            {
                RuntimeId = runtimeId,
                UnitTypeId = unitTypeId,
                DisplayName = displayName,
                Level = level,
                FinalAttack = currentAttack,
                FinalHealth = currentHealth,
                FinalManaCost = finalMana,
                PassiveAbilityId = passiveAbilityId,
                UpgradeCount = upgradeCount,
                BasketId = finalBasketId
            };
            
            return result;
        }
    }
}