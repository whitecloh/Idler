using System.Collections.Generic;
using Plinko.Scripts.Data.Enemies;
using Plinko.Scripts.Data.Levels;
using Plinko.Scripts.Models;
using UnityEngine;

namespace Plinko.Scripts.Services
{
    public sealed class EnemyWaveSelectionService
    {
        public EnemyWaveModel SelectWave(LevelData levelData, int currentEnemyBaseHealth)
        {
            if (levelData == null || levelData.HpThresholdWaves == null || levelData.HpThresholdWaves.Count == 0)
            {
                return new EnemyWaveModel
                {
                    Enemies = new List<EnemyUnitSnapshotModel>()
                };
            }

            var maxHealth = Mathf.Max(1, levelData.EnemyBaseMaxHealth);
            var currentPercent = Mathf.Clamp(Mathf.CeilToInt(currentEnemyBaseHealth * 100f / maxHealth), 0, 100);
            EnemyWaveThresholdData best = null;
            EnemyWaveThresholdData fallback = null;
            foreach (var wave in levelData.HpThresholdWaves)
            {
                if (wave == null)
                {
                    continue;
                }

                if (fallback == null || wave.ThresholdPercent > fallback.ThresholdPercent)
                {
                    fallback = wave;
                }

                if (currentPercent <= wave.ThresholdPercent)
                {
                    if (best == null || wave.ThresholdPercent < best.ThresholdPercent)
                    {
                        best = wave;
                    }
                }
            }

            if (best == null)
            {
                best = fallback;
            }

            var model = new EnemyWaveModel
            {
                ThresholdPercent = best != null ? best.ThresholdPercent : 0,
                Enemies = new List<EnemyUnitSnapshotModel>()
            };

            if (best == null || best.Enemies == null)
            {
                return model;
            }

            foreach (var enemy in best.Enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                model.Enemies.Add(new EnemyUnitSnapshotModel
                {
                    SpawnId = enemy.Id,
                    DisplayName = enemy.DisplayName,
                    Attack = enemy.Attack,
                    Health = enemy.Health,
                    BoardX = enemy.BoardX,
                    BoardY = enemy.BoardY,
                    MoveRange = enemy.MoveRange,
                    AttackRange = enemy.AttackRange
                });
                model.TotalAttack += enemy.Attack;
                model.TotalHealth += enemy.Health;
            }

            return model;
        }
    }
}
