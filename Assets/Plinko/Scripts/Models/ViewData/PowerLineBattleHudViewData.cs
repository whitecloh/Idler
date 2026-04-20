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
        public List<PowerLineUnitSpawnedEventViewData> UnitSpawnEvents = new();
        public List<PowerLineAttackEventViewData> AttackEvents = new();
        public List<PowerLineDamageEventViewData> DamageEvents = new();
        public List<PowerLinePlugEventViewData> PlugEvents = new();
        public List<PowerLineLaneConnectedEventViewData> LaneConnectedEvents = new();
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
        public int MaxHealth;
        public int ManaCost;
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
        public Enums.AttackType AttackType;
        public int LaneIndex;
        public float NormalizedPosition;
        public bool IsEnemy;
        public bool IsCarryingPlug;
        public Sprite PortraitSprite;
        public Sprite ProjectileSprite;
        public CharacterAnimationSetData BattleAnimations;
    }

    [Serializable]
    public sealed class PowerLineUnitSpawnedEventViewData
    {
        public int RuntimeId;
        public bool IsEnemy;
        public int LaneIndex;
        public float NormalizedPosition;
    }

    [Serializable]
    public sealed class PowerLineAttackEventViewData
    {
        public int AttackerRuntimeId;
        public bool AttackerIsEnemy;
        public bool TargetIsBase;
        public int LaneIndex;
        public float StartNormalizedPosition;
        public float TargetNormalizedPosition;
        public Enums.AttackType AttackType;
        public Sprite ProjectileSprite;
    }

    [Serializable]
    public sealed class PowerLineDamageEventViewData
    {
        public int TargetRuntimeId;
        public bool TargetIsEnemy;
        public bool TargetIsBase;
        public int LaneIndex;
        public float NormalizedPosition;
        public int Amount;
    }

    [Serializable]
    public sealed class PowerLinePlugEventViewData
    {
        public int LaneIndex;
        public PowerLinePlugStatus Status;
        public float NormalizedPosition;
        public int CarrierRuntimeId;
    }

    [Serializable]
    public sealed class PowerLineLaneConnectedEventViewData
    {
        public int LaneIndex;
    }
}
