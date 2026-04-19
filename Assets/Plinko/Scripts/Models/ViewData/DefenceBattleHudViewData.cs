using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class DefenceBattleHudViewData
    {
        public string LevelKey;
        public string LocationDisplayName;
        public Plinko.Scripts.Data.Common.Enums.PhaseType Phase;
        public int CurrentMana;
        public int MaxMana;
        public int CurrentTurn;
        public int BaseDefenseCompletedTurns;
        public int BaseDefenseRequiredTurns;
        public int BaseDefenseLaneCount;
        public int BaseDefenseCellsPerLane;
        public int BaseDefensePlayerSideCellCount;
        public string ActiveEnemyWaveDebug;
        public string StatusText;
        public bool CanDeploy;
        public bool CanStartBattle;
        public bool IsBattleResolved;
        public bool IsInteractionLocked;
        public Sprite BackgroundSprite;
        public BattleBaseViewData PlayerBase = new();
        public List<PurchaseLevelProgressEntryViewData> Levels = new();
        public List<HandCardViewData> HandCards = new();
        public List<BattleDeckUnitViewData> DeckUnits = new();
        public List<BattleBoardUnitViewData> PlayerUnits = new();
        public List<BattleBoardUnitViewData> EnemyUnits = new();
        public List<BattleBoardUnitViewData> NextWaveUnits = new();
    }
}
