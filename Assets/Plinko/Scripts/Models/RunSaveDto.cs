using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Common;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class RunSaveDto
    {
        public string LocationId;
        public int LevelIndex;
        public Enums.LevelType LevelType;
        public Enums.PhaseType PhaseType;
        public int Gold;
        public int PlayerBaseHealth;
        public int EnemyBaseHealth;
        public int PurchaseRerollCount;
        public int PinRerollCount;
        public bool HasActiveRun;
        public List<OwnedUnitSaveDto> OwnedUnits = new();
        public PlinkoBoardSaveDto Board = new();
    }
}