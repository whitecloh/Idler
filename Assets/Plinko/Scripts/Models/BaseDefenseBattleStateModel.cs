using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BaseDefenseBattleStateModel
    {
        public int LaneCount;
        public int CellsPerLane;
        public int PlayerSideCellCount;
        public int StartingMana;
        public int MaxMana;
        public int RequiredTurnCount;
        public int CompletedTurnCount;
        public int CurrentManaCap;
        public int NextRuntimeId = 1;
        public List<BaseDefenseUnitStateModel> PlayerUnits = new();
        public List<BaseDefenseUnitStateModel> EnemyUnits = new();
        public List<BaseDefenseWavePreviewUnitModel> PreviewWaveUnits = new();
    }
}
