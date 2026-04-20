using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Visuals;
using UnityEngine;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class PowerLineBattleStateModel
    {
        public string LevelTitle;
        public float LaneLength;
        public int CurrentTick;
        public int CurrentMana;
        public int StartingMana;
        public int MaxMana;
        public int ManaPerTick;
        public int ManaTickInterval;
        public int RerollManaCost;
        public int NextRuntimeId = 1;
        public float TickAccumulator;
        public List<int> DeckOwnedUnitRuntimeIds = new();
        public List<PowerLineLaneStateModel> Lanes = new();
        public List<PowerLineUnitStateModel> PlayerUnits = new();
        public List<PowerLineUnitStateModel> EnemyUnits = new();
        public List<PowerLineSpawnSnapshotModel> PendingSpawns = new();
    }

    [Serializable]
    public sealed class PowerLineLaneStateModel
    {
        public Enums.PowerLineLane Lane;
        public bool IsConnected;
        public PowerLinePlugStateModel Plug = new();
    }

    [Serializable]
    public sealed class PowerLinePlugStateModel
    {
        public PowerLinePlugStatus Status;
        public int CarrierRuntimeId;
        public float Position;
    }

    public enum PowerLinePlugStatus
    {
        AtSpawn = 0,
        Carried = 1,
        Dropped = 2,
        Connected = 3
    }

    [Serializable]
    public sealed class PowerLineUnitStateModel
    {
        public int RuntimeId;
        public int SourceOwnedUnitRuntimeId;
        public string SpawnId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public int MaxHealth;
        public int ManaCost;
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
        public Enums.AttackType AttackType;
        public float AttackAccumulator;
        public Enums.PowerLineLane Lane;
        public float Position;
        public bool IsEnemy;
        public bool IsCarryingPlug;
        public Sprite PortraitSprite;
        public Sprite ProjectileSprite;
        public CharacterAnimationSetData BattleAnimations;
    }

    [Serializable]
    public sealed class PowerLineSpawnSnapshotModel
    {
        public int TimeTick;
        public string SpawnId;
        public string DisplayName;
        public int Attack;
        public int Health;
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
        public Enums.AttackType AttackType;
        public Enums.PowerLineLane Lane;
        public Sprite PortraitSprite;
        public Sprite ProjectileSprite;
        public CharacterAnimationSetData BattleAnimations;
    }
}
