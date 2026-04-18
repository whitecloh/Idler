using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Common;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BattleHudViewData
    {
        public string LevelKey;
        public string LocationDisplayName;
        public Enums.PhaseType Phase;
        public int CurrentMana;
        public int MaxMana;
        public int PlayerBaseHealth;
        public int EnemyBaseHealth;
        public int CurrentTurn;
        public string ActiveEnemyWaveDebug;
        public string StatusText;
        public bool CanDeploy;
        public bool CanStartBattle;
        public bool IsBattleResolved;
        public bool IsInteractionLocked;
        public Sprite BackgroundSprite;
        public BattleBaseViewData PlayerBase = new();
        public BattleBaseViewData EnemyBase = new();
        public List<PurchaseLevelProgressEntryViewData> Levels = new();
        public List<HandCardViewData> HandCards = new();
        public List<BattleDeckUnitViewData> DeckUnits = new();
        public List<BattleBoardUnitViewData> PlayerUnits = new();
        public List<BattleBoardUnitViewData> EnemyUnits = new();
    }
}
