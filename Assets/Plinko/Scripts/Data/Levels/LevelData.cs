using System.Collections.Generic;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Enemies;
using UnityEngine;

namespace Plinko.Scripts.Data.Levels
{
    [CreateAssetMenu(menuName = "Session/Level", fileName = "LevelData")]
    public sealed class LevelData : ScriptableObject
    {
        [SerializeField] private string id = string.Empty;
        [SerializeField] private Enums.LevelType levelType;
        [SerializeField] private Sprite progressSprite;
        [SerializeField] private Sprite gridSprite;
        [SerializeField] private Sprite playerBaseSprite;
        [SerializeField] private Sprite enemyBaseSprite;
        [SerializeField] private Sprite backgroundSprite;
        [SerializeField] private LevelPhaseData preBattlePhase;
        [SerializeField] private int enemyBaseMaxHealth = 100;
        [SerializeField] private int victoryReward;
        [SerializeField] private List<EnemyWaveThresholdData> hpThresholdWaves = new();

        public string Id => id;
        public Enums.LevelType LevelType => levelType;
        public Sprite ProgressSprite => progressSprite;
        public Sprite GridSprite => gridSprite;
        public Sprite PlayerBaseSprite => playerBaseSprite;
        public Sprite EnemyBaseSprite => enemyBaseSprite;
        public Sprite BackgroundSprite => backgroundSprite;
        public LevelPhaseData PreBattlePhase => preBattlePhase;
        public int EnemyBaseMaxHealth => enemyBaseMaxHealth;
        public int VictoryReward => victoryReward;
        public IReadOnlyList<EnemyWaveThresholdData> HpThresholdWaves => hpThresholdWaves;
    }  
}
