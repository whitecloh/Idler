using System.Collections.Generic;
using Plinko.Scripts.Data.Meta;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Stats;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;
using UnityEngine.Serialization;

namespace Plinko.Scripts.Data.Units
{
    [CreateAssetMenu(menuName = "Session/UnitType", fileName = "UnitTypeData")]
    public sealed class UnitTypeData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private string description = string.Empty;
        [FormerlySerializedAs("icon")]
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private CharacterAnimationSetData battleAnimations = new();
        [SerializeField] private Enums.AttackType attackType = Enums.AttackType.Melee;
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private Sprite trainingFieldSprite;
        [SerializeField] private int defaultManaCost;
        [SerializeField] private List<StatValueEntryData> baseStats = new();
        [SerializeField] private bool canAttackOtherLines;
        [SerializeField] private bool canMoveBetweenLines;
        [SerializeField] private int shopPrice;
        [SerializeField] private int generationWeight = 1;
        [SerializeField] private PassiveAbilityData passiveAbility;
        [SerializeField] private UnlockConditionData unlockCondition;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public Sprite PortraitSprite => portraitSprite;
        public CharacterAnimationSetData BattleAnimations => battleAnimations;
        public Enums.AttackType AttackType => attackType;
        public Sprite ProjectileSprite => projectileSprite;
        public Sprite TrainingFieldSprite => trainingFieldSprite;
        public IReadOnlyList<StatValueEntryData> BaseStats => baseStats;
        public Sprite Icon => portraitSprite;
        public int BaseAttack => GetIntStat(StatTypeIds.Attack);
        public int BaseHealth => GetIntStat(StatTypeIds.Health);
        public int DefaultManaCost => defaultManaCost;
        public float BaseMoveSpeed => GetFloatStat(StatTypeIds.MoveSpeed, 0.4f);
        public int BattleMoveRange => GetIntStat(StatTypeIds.MoveRange, 1);
        public int BattleAttackRange => GetIntStat(StatTypeIds.AttackRange, 1);
        public float BaseAttackSpeed => GetFloatStat(StatTypeIds.AttackSpeed, 0.5f);
        public bool CanAttackOtherLines => canAttackOtherLines;
        public bool CanMoveBetweenLines => canMoveBetweenLines;
        public int PassiveIncomePerTick => GetIntStat(StatTypeIds.PassiveIncome);
        public int ShopPrice => shopPrice;
        public int GenerationWeight => generationWeight;
        public PassiveAbilityData PassiveAbility => passiveAbility;
        public UnlockConditionData UnlockCondition => unlockCondition;

        public bool TryGetBaseStatValue(string statTypeId, out float value)
        {
            if (baseStats != null)
            {
                for (var index = 0; index < baseStats.Count; index++)
                {
                    var entry = baseStats[index];
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

        private int GetIntStat(string statTypeId, int fallback = 0)
        {
            return TryGetBaseStatValue(statTypeId, out var value)
                ? Mathf.RoundToInt(value)
                : fallback;
        }

        private float GetFloatStat(string statTypeId, float fallback)
        {
            return TryGetBaseStatValue(statTypeId, out var value)
                ? value
                : fallback;
        }
    }
}
