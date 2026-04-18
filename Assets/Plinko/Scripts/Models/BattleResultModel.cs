using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BattleResultModel
    {
        public int PlayerBaseHealthBefore;
        public int PlayerBaseHealthAfter;
        public int EnemyBaseHealthBefore;
        public int EnemyBaseHealthAfter;
        public int EnemyKillsThisTurn;
        public int EnemyKillsTotal;
        public int DamageToEnemyBaseThisTurn;
        public int DamageToEnemyBaseTotal;
        public int DamageToPlayerBaseThisTurn;
        public int DamageToPlayerBaseTotal;
        public int TurnsSpent;
        public int BaseReward;
        public int RewardGranted;
        public bool IsVictory;
        public bool IsDefeat;
    }
}
