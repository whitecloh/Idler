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
        [SerializeField] private int attack;
        [SerializeField] private int health;
        [SerializeField] private int boardX;
        [SerializeField] private int boardY;
        [SerializeField] private int moveRange = 1;
        [SerializeField] private int attackRange = 1;
        [SerializeField] private float moveSpeed = 0.4f;
        [SerializeField] private float attackSpeed = 0.5f;
        [SerializeField] private Enums.AttackType attackType = Enums.AttackType.Melee;
        [SerializeField] private Sprite projectileSprite;
        [SerializeField] private bool canAttackOtherLines;
        [SerializeField] private bool canMoveBetweenLines;
        [SerializeField] private Sprite portraitSprite;
        [SerializeField] private CharacterAnimationSetData battleAnimations = new();
        [SerializeField] private Sprite trainingFieldSprite;

        public string Id => id;
        public string DisplayName => displayName;
        public int Attack => attack;
        public int Health => health;
        public int BoardX => boardX;
        public int BoardY => boardY;
        public int MoveRange => moveRange;
        public int AttackRange => attackRange;
        public float MoveSpeed => moveSpeed;
        public float AttackSpeed => attackSpeed;
        public Enums.AttackType AttackType => attackType;
        public Sprite ProjectileSprite => projectileSprite;
        public bool CanAttackOtherLines => canAttackOtherLines;
        public bool CanMoveBetweenLines => canMoveBetweenLines;
        public Sprite PortraitSprite => portraitSprite;
        public CharacterAnimationSetData BattleAnimations => battleAnimations;
        public Sprite TrainingFieldSprite => trainingFieldSprite;
    }
}
