using System;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BattleResultViewData
    {
        public string Title;
        public string Description;
        public string RewardText;
        public string RewardBreakdownText;
        public string PrimaryActionLabel;
        public bool IsVictory;
        public bool IsDefeat;
        public bool IsRunCompleted;
        public int PlayerBaseHealthAfter;
        public int EnemyBaseHealthAfter;
        public bool CanAdvance;
        public bool CanReturnToMenu;
    }
}
