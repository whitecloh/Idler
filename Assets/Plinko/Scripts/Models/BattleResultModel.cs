using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BattleResultModel
    {
        public int PlayerBaseHealthAfter;
        public int EnemyBaseHealthAfter;
        public bool IsVictory;
        public bool IsDefeat;
    }
}