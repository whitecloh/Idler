using System.Collections.Generic;
using System.Linq;
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
                unitType.PassiveAbility != null ? unitType.PassiveAbility.Id : string.Empty,
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
            var currentX = 0f;
            var currentAttack = baseAttack;
            var currentHealth = baseHealth;
            var currentManaModifier = 0;
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
                        ? GetStartColumn(rowCount, field.HorizontalSpacing)
                        : ChooseNextColumn(currentX, rowCount, field.HorizontalSpacing);
                    currentX = GetColumnX(currentColumn, rowCount, field.HorizontalSpacing);

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
                    currentManaModifier += node.ManaDelta;
                    result.Nodes.Add(node);
                    slotIndex += rowCount;
                }
            }
            
            var finalMana = Mathf.Max(1, baseManaCost);
            var basketMana = finalMana;
            var finalBasketId = string.Empty;
            if (field != null && field.Baskets != null && field.Baskets.Count > 0)
            {
                var basket = ChooseNearestBasket(currentX, field.Baskets, field.HorizontalSpacing);
                basketMana = basket != null ? basket.ManaValue : basketMana;
                finalMana = Mathf.Max(1, basketMana + currentManaModifier);
                finalBasketId = basket != null ? basket.Id : string.Empty;
            }
            else
            {
                finalMana = Mathf.Max(1, baseManaCost + currentManaModifier);
            }

            result.FinalBasketId = finalBasketId;
            result.FinalBasketManaValue = basketMana;
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

        private static int GetStartColumn(int rowCount, float horizontalSpacing)
        {
            if (rowCount <= 1)
            {
                return 0;
            }

            return ChooseNearestColumns(0f, rowCount, horizontalSpacing).First();
        }

        private static int ChooseNextColumn(float currentX, int nextRowCount, float horizontalSpacing)
        {
            var candidates = ChooseNearestColumns(currentX, nextRowCount, horizontalSpacing);
            if (candidates.Count <= 1)
            {
                return candidates.Count == 0 ? 0 : candidates[0];
            }

            return candidates[UnityEngine.Random.Range(0, candidates.Count)];
        }

        private static List<int> ChooseNearestColumns(float currentX, int rowCount, float horizontalSpacing)
        {
            var candidates = new List<(int ColumnIndex, float Distance)>(rowCount);
            for (var columnIndex = 0; columnIndex < rowCount; columnIndex++)
            {
                var columnX = GetColumnX(columnIndex, rowCount, horizontalSpacing);
                candidates.Add((columnIndex, Mathf.Abs(columnX - currentX)));
            }

            candidates.Sort((left, right) =>
            {
                var distanceCompare = left.Distance.CompareTo(right.Distance);
                return distanceCompare != 0 ? distanceCompare : left.ColumnIndex.CompareTo(right.ColumnIndex);
            });

            var result = new List<int>();
            foreach (var candidate in candidates)
            {
                if (result.Count == 0)
                {
                    result.Add(candidate.ColumnIndex);
                    continue;
                }

                if (result.Count >= 2)
                {
                    break;
                }

                var firstDistance = Mathf.Abs(GetColumnX(result[0], rowCount, horizontalSpacing) - currentX);
                if (!Mathf.Approximately(candidate.Distance, firstDistance) && candidate.Distance > firstDistance)
                {
                    result.Add(candidate.ColumnIndex);
                    break;
                }

                if (!result.Contains(candidate.ColumnIndex))
                {
                    result.Add(candidate.ColumnIndex);
                }
            }

            if (result.Count == 0)
            {
                result.Add(0);
            }

            return result;
        }

        private static float GetColumnX(int columnIndex, int rowCount, float horizontalSpacing)
        {
            var centeredOffset = (rowCount - 1) * 0.5f;
            return (columnIndex - centeredOffset) * Mathf.Max(0.0001f, horizontalSpacing);
        }

        private static BasketTypeData ChooseNearestBasket(float currentX, IReadOnlyList<BasketTypeData> baskets, float horizontalSpacing)
        {
            if (baskets == null || baskets.Count == 0)
            {
                return null;
            }

            var candidates = new List<(BasketTypeData Basket, float Distance, int Index)>(baskets.Count);
            for (var index = 0; index < baskets.Count; index++)
            {
                candidates.Add((baskets[index], Mathf.Abs(GetColumnX(index, baskets.Count, horizontalSpacing) - currentX), index));
            }

            candidates.Sort((left, right) =>
            {
                var distanceCompare = left.Distance.CompareTo(right.Distance);
                return distanceCompare != 0 ? distanceCompare : left.Index.CompareTo(right.Index);
            });

            var nearest = new List<BasketTypeData>();
            for (var index = 0; index < candidates.Count && nearest.Count < 2; index++)
            {
                if (candidates[index].Basket != null)
                {
                    nearest.Add(candidates[index].Basket);
                }
            }

            if (nearest.Count == 0)
            {
                return null;
            }

            if (nearest.Count == 1)
            {
                return nearest[0];
            }

            var totalWeight = 0;
            foreach (var basket in nearest)
            {
                totalWeight += Mathf.Max(0, basket.GenerationWeight);
            }

            if (totalWeight <= 0)
            {
                return nearest[UnityEngine.Random.Range(0, nearest.Count)];
            }

            var roll = UnityEngine.Random.Range(0, totalWeight);
            var accumulated = 0;
            foreach (var basket in nearest)
            {
                accumulated += Mathf.Max(0, basket.GenerationWeight);
                if (roll < accumulated)
                {
                    return basket;
                }
            }

            return nearest[^1];
        }
    }
}
