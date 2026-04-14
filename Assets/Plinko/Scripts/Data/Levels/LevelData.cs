using Plinko.Scripts.Data.Common;
using UnityEngine;

namespace Plinko.Scripts.Data.Levels
{
    [CreateAssetMenu(menuName = "Session/Level", fileName = "LevelData")]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private Enums.LevelType levelType;
        [SerializeField] private int enemyBaseHealth;
        [SerializeField] private int victoryReward;

        public string Id => id;
        public Enums.LevelType LevelType => levelType;
        public int EnemyBaseHealth => enemyBaseHealth;
        public int VictoryReward => victoryReward;
    }   
}