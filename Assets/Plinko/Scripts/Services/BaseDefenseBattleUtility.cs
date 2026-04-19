using System.Collections.Generic;
using Plinko.Scripts.Data.Battle;
using Plinko.Scripts.Data.Enemies;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public static class BaseDefenseBattleUtility
    {
        public static BaseDefenseBattleStateModel CreateState(LevelData levelData)
        {
            if (levelData == null || levelData.BaseDefenseBattle == null)
            {
                return null;
            }

            var battleData = levelData.BaseDefenseBattle;
            return new BaseDefenseBattleStateModel
            {
                LaneCount = Mathf.Max(1, battleData.LaneCount),
                CellsPerLane = Mathf.Max(1, battleData.CellsPerLane),
                PlayerSideCellCount = Mathf.Clamp(battleData.PlayerSideCellCount, 1, Mathf.Max(1, battleData.CellsPerLane)),
                StartingMana = Mathf.Max(0, battleData.StartingMana),
                MaxMana = Mathf.Max(0, battleData.MaxMana),
                RequiredTurnCount = Mathf.Max(0, battleData.RequiredTurnCount),
                CompletedTurnCount = 0,
                CurrentManaCap = Mathf.Max(0, battleData.StartingMana),
                NextRuntimeId = 1,
                PlayerUnits = new List<BaseDefenseUnitStateModel>(),
                EnemyUnits = new List<BaseDefenseUnitStateModel>(),
                PreviewWaveUnits = BuildPreviewWaveUnits(levelData, 1)
            };
        }

        public static List<BaseDefenseWavePreviewUnitModel> BuildPreviewWaveUnits(LevelData levelData, int turnIndex)
        {
            var previewUnits = new List<BaseDefenseWavePreviewUnitModel>();
            if (levelData == null || levelData.BaseDefenseBattle == null || levelData.BaseDefenseBattle.Waves == null)
            {
                return previewUnits;
            }

            foreach (var wave in levelData.BaseDefenseBattle.Waves)
            {
                if (wave == null || wave.TurnIndex != turnIndex || wave.Spawns == null)
                {
                    continue;
                }

                foreach (var spawn in wave.Spawns)
                {
                    if (spawn?.Enemy == null)
                    {
                        continue;
                    }

                    previewUnits.Add(CreatePreviewUnit(spawn.Enemy, spawn.LaneIndex, spawn.EnemySideCellIndex));
                }

                break;
            }

            return previewUnits;
        }

        public static BaseDefenseWavePreviewUnitModel CreatePreviewUnit(EnemyUnitSpawnData enemyData, int laneIndex, int enemySideCellIndex)
        {
            return new BaseDefenseWavePreviewUnitModel
            {
                SpawnId = enemyData.Id,
                DisplayName = enemyData.DisplayName,
                Attack = Mathf.Max(0, enemyData.Attack),
                Health = Mathf.Max(0, enemyData.Health),
                MoveRange = Mathf.Max(1, enemyData.MoveRange),
                AttackRange = Mathf.Max(0, enemyData.AttackRange),
                MoveSpeed = Mathf.Max(0f, enemyData.MoveSpeed),
                AttackSpeed = Mathf.Max(0f, enemyData.AttackSpeed),
                CanAttackOtherLines = enemyData.CanAttackOtherLines,
                CanMoveBetweenLines = enemyData.CanMoveBetweenLines,
                LaneIndex = laneIndex,
                EnemySideCellIndex = enemySideCellIndex,
                PortraitSprite = enemyData.PortraitSprite,
                BattleAnimations = enemyData.BattleAnimations
            };
        }

        public static int GetAbsoluteCellIndex(BaseDefenseBattleStateModel state, int enemySideCellIndex)
        {
            if (state == null)
            {
                return 0;
            }

            var clampedIndex = Mathf.Clamp(enemySideCellIndex, 0, Mathf.Max(0, state.CellsPerLane - 1));
            return state.CellsPerLane - 1 - clampedIndex;
        }

        public static BaseDefenseBattleStateModel CloneState(BaseDefenseBattleStateModel source)
        {
            if (source == null)
            {
                return null;
            }

            var clone = new BaseDefenseBattleStateModel
            {
                LaneCount = source.LaneCount,
                CellsPerLane = source.CellsPerLane,
                PlayerSideCellCount = source.PlayerSideCellCount,
                StartingMana = source.StartingMana,
                MaxMana = source.MaxMana,
                RequiredTurnCount = source.RequiredTurnCount,
                CompletedTurnCount = source.CompletedTurnCount,
                CurrentManaCap = source.CurrentManaCap,
                NextRuntimeId = source.NextRuntimeId,
                PlayerUnits = new List<BaseDefenseUnitStateModel>(),
                EnemyUnits = new List<BaseDefenseUnitStateModel>(),
                PreviewWaveUnits = new List<BaseDefenseWavePreviewUnitModel>()
            };

            foreach (var unit in source.PlayerUnits)
            {
                clone.PlayerUnits.Add(CloneUnit(unit));
            }

            foreach (var unit in source.EnemyUnits)
            {
                clone.EnemyUnits.Add(CloneUnit(unit));
            }

            foreach (var previewUnit in source.PreviewWaveUnits)
            {
                clone.PreviewWaveUnits.Add(ClonePreviewUnit(previewUnit));
            }

            return clone;
        }

        public static BaseDefenseUnitStateModel CloneUnit(BaseDefenseUnitStateModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new BaseDefenseUnitStateModel
            {
                RuntimeId = source.RuntimeId,
                SourceOwnedUnitRuntimeId = source.SourceOwnedUnitRuntimeId,
                SpawnId = source.SpawnId,
                DisplayName = source.DisplayName,
                Attack = source.Attack,
                Health = source.Health,
                ManaCost = source.ManaCost,
                MoveRange = source.MoveRange,
                AttackRange = source.AttackRange,
                MoveSpeed = source.MoveSpeed,
                AttackSpeed = source.AttackSpeed,
                CanAttackOtherLines = source.CanAttackOtherLines,
                CanMoveBetweenLines = source.CanMoveBetweenLines,
                LaneIndex = source.LaneIndex,
                CellIndex = source.CellIndex,
                IsEnemy = source.IsEnemy,
                PortraitSprite = source.PortraitSprite,
                BattleAnimations = source.BattleAnimations
            };
        }

        public static BaseDefenseWavePreviewUnitModel ClonePreviewUnit(BaseDefenseWavePreviewUnitModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new BaseDefenseWavePreviewUnitModel
            {
                SpawnId = source.SpawnId,
                DisplayName = source.DisplayName,
                Attack = source.Attack,
                Health = source.Health,
                MoveRange = source.MoveRange,
                AttackRange = source.AttackRange,
                MoveSpeed = source.MoveSpeed,
                AttackSpeed = source.AttackSpeed,
                CanAttackOtherLines = source.CanAttackOtherLines,
                CanMoveBetweenLines = source.CanMoveBetweenLines,
                LaneIndex = source.LaneIndex,
                EnemySideCellIndex = source.EnemySideCellIndex,
                PortraitSprite = source.PortraitSprite,
                BattleAnimations = source.BattleAnimations
            };
        }

        public static BaseDefenseUnitSaveDto ToSaveDto(BaseDefenseUnitStateModel source)
        {
            if (source == null)
            {
                return null;
            }

            return new BaseDefenseUnitSaveDto
            {
                RuntimeId = source.RuntimeId,
                SourceOwnedUnitRuntimeId = source.SourceOwnedUnitRuntimeId,
                SpawnId = source.SpawnId,
                DisplayName = source.DisplayName,
                Attack = source.Attack,
                Health = source.Health,
                ManaCost = source.ManaCost,
                MoveRange = source.MoveRange,
                AttackRange = source.AttackRange,
                MoveSpeed = source.MoveSpeed,
                AttackSpeed = source.AttackSpeed,
                CanAttackOtherLines = source.CanAttackOtherLines,
                CanMoveBetweenLines = source.CanMoveBetweenLines,
                LaneIndex = source.LaneIndex,
                CellIndex = source.CellIndex,
                IsEnemy = source.IsEnemy
            };
        }
    }
}
