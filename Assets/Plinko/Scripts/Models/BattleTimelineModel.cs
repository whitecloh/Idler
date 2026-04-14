using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BattleTimelineModel
    {
        public List<BattleActionModel> Actions = new();
    }
}