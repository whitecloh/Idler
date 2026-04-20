using System.Collections.Generic;
using Plinko.Scripts.Data.Stats;
using Plinko.Scripts.Data.Visuals;
using Plinko.Scripts.Data.Common;
using UnityEngine;

namespace Plinko.Scripts.Data.Enemies
{
    [CreateAssetMenu(menuName = "Session/EnemyUnitSpawn", fileName = "EnemyUnitSpawnData")]
    public sealed class EnemyUnitSpawnData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private string displayName = string.Empty;
        [SerializeField] private List<StatValueEntryData> baseStats = new();
        [SerializeField] private int boardX;
        [SerializeField] private int boardY;
        [SerializeField] private Enums.AttackType attackType = Enums.AttackType.Melee;
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private bool canAttackOtherLines;
        [SerializeField] private bool canMoveBetweenLines;
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private CharacterAnimationSetData battleAnimations = new();
        [SerializeField] private Sprite trainingFieldSprite;

        public string Id => id;
        public string DisplayName => displayName;
        public IReadOnlyList<StatValueEntryData> BaseStats => baseStats;
        public int Attack => GetIntStat(StatTypeIds.Attack);
        public int Health => GetIntStat(StatTypeIds.Health);
        public int BoardX => boardX;
        public int BoardY => boardY;
        public int MoveRange => GetIntStat(StatTypeIds.MoveRange, 1);
        public int AttackRange => GetIntStat(StatTypeIds.AttackRange, 1);
        public float MoveSpeed => GetFloatStat(StatTypeIds.MoveSpeed, 0.4f);
        public float AttackSpeed => GetFloatStat(StatTypeIds.AttackSpeed, 0.5f);
        public Enums.AttackType AttackType => attackType;
        public Sprite ProjectileSprite => projectileSprite;
        public bool CanAttackOtherLines => canAttackOtherLines;
        public bool CanMoveBetweenLines => canMoveBetweenLines;
        public Sprite PortraitSprite => portraitSprite;
        public CharacterAnimationSetData BattleAnimations => battleAnimations;
        public Sprite TrainingFieldSprite => trainingFieldSprite;

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
