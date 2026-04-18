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
        public Enums.RunStatus RunStatus;
        public int Gold;
        public int CurrentMana;
        public int PlayerBaseHealth;
        public int EnemyBaseHealth;
        public int BattleTurn;
        public int HandNextRuntimeId;
        public int NextDeploymentOrder;
        public int BattleEnemyKillsTotal;
        public int BattleDamageToEnemyBaseTotal;
        public int BattleDamageToPlayerBaseTotal;
        public int PurchaseRerollCount;
        public int PinRerollCount;
        public bool HasActiveRun;
        public BattleResultModel BattleResult;
        public List<OwnedUnitSaveDto> OwnedUnits = new();
        public List<HandCardSaveDto> HandCards = new();
        public List<DeployedUnitSaveDto> DeployedUnits = new();
        public PlinkoBoardSaveDto Board = new();
    }
}
