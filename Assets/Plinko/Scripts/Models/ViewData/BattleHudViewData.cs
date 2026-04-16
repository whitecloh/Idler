using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BattleHudViewData
    {
        public int CurrentMana;
        public int PlayerBaseHealth;
        public int EnemyBaseHealth;
        public int CurrentTurn;
        public string ActiveEnemyWaveDebug;
        public bool IsBattleResolved;
        public List<HandCardViewData> HandCards = new();
    }
}