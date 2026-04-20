using System.Collections.Generic;
using Plinko.Scripts.Data.Battle;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Enemies;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public static class PowerLineBattleUtility
    {
        public static PowerLineBattleStateModel CreateState(LevelData levelData, GameSettingsService settings)
        {
            if (levelData == null || levelData.PowerLineBattle == null || settings == null)
            {
                return null;
            }

            var state = new PowerLineBattleStateModel
            {
                LevelTitle = !string.IsNullOrWhiteSpace(levelData.DisplayName) ? levelData.DisplayName : levelData.Id,
                LaneLength = Mathf.Max(1f, levelData.PowerLineBattle.LaneLength),
                CurrentTick = 0,
                CurrentMana = Mathf.Max(0, settings.GetPowerLineStartingMana()),
                StartingMana = Mathf.Max(0, settings.GetPowerLineStartingMana()),
                MaxMana = Mathf.Max(0, settings.GetPowerLineMaxMana()),
                ManaPerTick = Mathf.Max(0, settings.GetPowerLineManaPerTick()),
                ManaTickInterval = Mathf.Max(1, settings.GetPowerLineManaTickInterval()),
                RerollManaCost = Mathf.Max(0, settings.GetPowerLineRerollManaCost()),
                NextRuntimeId = 1,
                TickAccumulator = 0f,
                Lanes = new List<PowerLineLaneStateModel>(),
                PlayerUnits = new List<PowerLineUnitStateModel>(),
                EnemyUnits = new List<PowerLineUnitStateModel>(),
                PendingSpawns = new List<PowerLineSpawnSnapshotModel>()
            };

            state.CurrentMana = Mathf.Clamp(state.CurrentMana, 0, state.MaxMana);

            foreach (Enums.PowerLineLane lane in System.Enum.GetValues(typeof(Enums.PowerLineLane)))
            {
                state.Lanes.Add(new PowerLineLaneStateModel
                {
                    Lane = lane,
                    IsConnected = false,
                    Plug = new PowerLinePlugStateModel
                    {
                        Status = PowerLinePlugStatus.AtSpawn,
                        CarrierRuntimeId = 0,
                        Position = 0f
                    }
                });
            }

            if (levelData.PowerLineBattle.Spawns != null)
            {
                foreach (var spawn in levelData.PowerLineBattle.Spawns)
                {
                    if (spawn?.Enemy == null)
                    {
                        continue;
                    }

                    state.PendingSpawns.Add(CreateSpawnSnapshot(spawn, state.LaneLength));
                }

                state.PendingSpawns.Sort((left, right) =>
                {
                    var tickCompare = left.TimeTick.CompareTo(right.TimeTick);
                    return tickCompare != 0 ? tickCompare : left.Lane.CompareTo(right.Lane);
                });
            }

            return state;
        }

        public static PowerLineSpawnSnapshotModel CreateSpawnSnapshot(PowerLineSpawnEntryData spawn, float laneLength)
        {
            var enemy = spawn.Enemy;
            return new PowerLineSpawnSnapshotModel
            {
                TimeTick = Mathf.Max(0, spawn.TimeTick),
                SpawnId = enemy.Id,
                DisplayName = enemy.DisplayName,
                Attack = Mathf.Max(0, enemy.Attack),
                Health = Mathf.Max(1, enemy.Health),
                MoveSpeed = Mathf.Max(0f, enemy.MoveSpeed),
                AttackRange = Mathf.Max(0, enemy.AttackRange),
                AttackSpeed = Mathf.Max(0f, enemy.AttackSpeed),
                AttackType = enemy.AttackType,
                Lane = spawn.Lane,
                PortraitSprite = enemy.PortraitSprite,
                ProjectileSprite = enemy.ProjectileSprite,
                BattleAnimations = enemy.BattleAnimations
            };
        }

        public static PowerLineUnitStateModel CreatePlayerUnit(
            int runtimeId,
            int ownedUnitRuntimeId,
            string displayName,
            int attack,
            int health,
            int manaCost,
            float moveSpeed,
            int attackRange,
            float attackSpeed,
            Enums.AttackType attackType,
            Enums.PowerLineLane lane,
            Sprite portraitSprite,
            Sprite projectileSprite,
            Data.Visuals.CharacterAnimationSetData battleAnimations)
        {
            return new PowerLineUnitStateModel
            {
                RuntimeId = runtimeId,
                SourceOwnedUnitRuntimeId = ownedUnitRuntimeId,
                DisplayName = displayName,
                Attack = Mathf.Max(0, attack),
                Health = Mathf.Max(1, health),
                MaxHealth = Mathf.Max(1, health),
                ManaCost = Mathf.Max(0, manaCost),
                MoveSpeed = Mathf.Max(0f, moveSpeed),
                AttackRange = Mathf.Max(0, attackRange),
                AttackSpeed = Mathf.Max(0f, attackSpeed),
                AttackType = attackType,
                AttackAccumulator = 0f,
                Lane = lane,
                Position = 0f,
                IsEnemy = false,
                IsCarryingPlug = false,
                PortraitSprite = portraitSprite,
                ProjectileSprite = projectileSprite,
                BattleAnimations = battleAnimations
            };
        }

        public static PowerLineUnitStateModel CreateEnemyUnit(PowerLineBattleStateModel state, PowerLineSpawnSnapshotModel spawn)
        {
            return new PowerLineUnitStateModel
            {
                RuntimeId = state.NextRuntimeId++,
                SpawnId = spawn.SpawnId,
                DisplayName = spawn.DisplayName,
                Attack = spawn.Attack,
                Health = spawn.Health,
                MaxHealth = spawn.Health,
                ManaCost = 0,
                MoveSpeed = spawn.MoveSpeed,
                AttackRange = spawn.AttackRange,
                AttackSpeed = spawn.AttackSpeed,
                AttackType = spawn.AttackType,
                AttackAccumulator = 0f,
                Lane = spawn.Lane,
                Position = state.LaneLength,
                IsEnemy = true,
                IsCarryingPlug = false,
                PortraitSprite = spawn.PortraitSprite,
                ProjectileSprite = spawn.ProjectileSprite,
                BattleAnimations = spawn.BattleAnimations
            };
        }

        public static PowerLineLaneStateModel GetLane(PowerLineBattleStateModel state, Enums.PowerLineLane lane)
        {
            if (state?.Lanes == null)
            {
                return null;
            }

            for (var index = 0; index < state.Lanes.Count; index++)
            {
                if (state.Lanes[index].Lane == lane)
                {
                    return state.Lanes[index];
                }
            }

            return null;
        }

        public static int GetConnectedLaneCount(PowerLineBattleStateModel state)
        {
            if (state?.Lanes == null)
            {
                return 0;
            }

            var connectedCount = 0;
            for (var index = 0; index < state.Lanes.Count; index++)
            {
                if (state.Lanes[index].IsConnected)
                {
                    connectedCount++;
                }
            }

            return connectedCount;
        }
    }
}
