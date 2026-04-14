using System;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BattleResultViewData
    {
        public bool IsVictory;
        public bool IsDefeat;
        public bool IsRunCompleted;
        public int PlayerBaseHealthAfter;
        public int EnemyBaseHealthAfter;
        public bool CanAdvance;
        public bool CanReturnToMenu;
    }
}