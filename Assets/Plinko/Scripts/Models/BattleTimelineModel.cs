using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BattleTimelineModel
    {
        public List<BattleTickModel> Ticks = new();
        public int SurvivorDamageToEnemyBase;
        public int SurvivorDamageToPlayerBase;
    }
}