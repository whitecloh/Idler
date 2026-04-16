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

        public string Id => id;
        public string DisplayName => displayName;
        public int Attack => attack;
        public int Health => health;
        public int BoardX => boardX;
        public int BoardY => boardY;
        public int MoveRange => moveRange;
        public int AttackRange => attackRange;
    }
}