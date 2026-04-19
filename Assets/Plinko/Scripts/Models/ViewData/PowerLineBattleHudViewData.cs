using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Visuals;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class PowerLineBattleHudViewData
    {
        public string LevelKey;
        public string LevelTitle;
        public string LocationDisplayName;
        public Enums.PhaseType Phase;
        public int CurrentMana;
        public int MaxMana;
        public int RerollManaCost;
        public bool CanReroll;
        public bool IsInteractionLocked;
        public Sprite BackgroundSprite;
        public BattleBaseViewData PlayerBase = new();
        public Sprite EnemyBaseSprite;
        public int ConnectedLaneCount;
        public int RequiredLaneCount;
        public List<PurchaseLevelProgressEntryViewData> Levels = new();
        public List<HandCardViewData> HandCards = new();
        public List<BattleDeckUnitViewData> DeckUnits = new();
        public List<PowerLineLaneViewData> Lanes = new();
        public List<PowerLineUnitViewData> PlayerUnits = new();
        public List<PowerLineUnitViewData> EnemyUnits = new();
    }

    [Serializable]
    public sealed class PowerLineLaneViewData
    {
        public int LaneIndex;
        public Enums.PowerLineLane Lane;
        public bool IsConnected;
        public bool IsSpawnAvailable;
        public PowerLinePlugViewData Plug = new();
    }

    [Serializable]
    public sealed class PowerLinePlugViewData
    {
        public PowerLinePlugStatus Status;
        public float NormalizedPosition;
        public int CarrierRuntimeId;
    }

    [Serializable]
    public sealed class PowerLineUnitViewData
    {
        public int RuntimeId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public int ManaCost;
        public int LaneIndex;
        public float NormalizedPosition;
        public bool IsEnemy;
        public bool IsCarryingPlug;
        public Sprite PortraitSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
