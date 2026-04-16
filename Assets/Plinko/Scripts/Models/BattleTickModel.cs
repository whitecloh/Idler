using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BattleTickModel
    {
        public int TickIndex;
        public List<BattleActionModel> Actions = new();
    }
}