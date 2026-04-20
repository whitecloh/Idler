using System.Collections.Generic;
using Plinko.Scripts.Data.Meta;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Stats;
using UnityEngine;

namespace Plinko.Scripts.Data.Pins
{
    [CreateAssetMenu(menuName = "Session/PinType", fileName = "PinTypeData")]
    public sealed class PinTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string rarity = string.Empty;
        [SerializeField] private Sprite fieldSprite;
        [SerializeField] private int shopPrice;
        [SerializeField] private int generationWeight = 1;
        [SerializeField] private int manaModifier;
        [SerializeField] private List<StatValueEntryData> modifiers = new();
        [SerializeField] private UnlockConditionData unlockCondition;

        public string Id => id;
        public string DisplayName => displayName;
        public string Rarity => rarity;
        public Sprite FieldSprite => fieldSprite;
        public int ShopPrice => shopPrice;
        public int GenerationWeight => generationWeight;
        public IReadOnlyList<StatValueEntryData> Modifiers => modifiers;
        public int AttackModifier => GetIntModifier(StatTypeIds.Attack);
        public int HealthModifier => GetIntModifier(StatTypeIds.Health);
        public int ManaModifier => manaModifier;
        public float MoveSpeedModifier => GetFloatModifier(StatTypeIds.MoveSpeed);
        public int AttackRangeModifier => GetIntModifier(StatTypeIds.AttackRange);
        public float AttackSpeedModifier => GetFloatModifier(StatTypeIds.AttackSpeed);
        public UnlockConditionData UnlockCondition => unlockCondition;

        public bool TryGetModifierValue(string statTypeId, out float value)
        {
            if (modifiers != null)
            {
                for (var index = 0; index < modifiers.Count; index++)
                {
                    var entry = modifiers[index];
                    if (entry == null || entry.StatType == null || entry.StatType.Id != statTypeId)
                    {
                        continue;
                    }

                    value = entry.Value;
                    return true;
                }
            }

            value = 0f;
            return false;
        }

        private int GetIntModifier(string statTypeId, int fallback = 0)
        {
            return TryGetModifierValue(statTypeId, out var value)
                ? Mathf.RoundToInt(value)
                : fallback;
        }

        private float GetFloatModifier(string statTypeId, float fallback = 0f)
        {
            return TryGetModifierValue(statTypeId, out var value)
                ? value
                : fallback;
        }
    }
}
