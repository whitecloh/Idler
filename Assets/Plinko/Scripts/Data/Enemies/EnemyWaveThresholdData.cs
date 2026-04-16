using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Data.Enemies
{
    [CreateAssetMenu(menuName = "Session/EnemyWaveThreshold", fileName = "EnemyWaveThresholdData")]
    public sealed class EnemyWaveThresholdData : ScriptableObject
    {
        [SerializeField] private int thresholdPercent = 100;
        [SerializeField] private List<EnemyUnitSpawnData> enemies = new();

        public int ThresholdPercent => thresholdPercent;
        public IReadOnlyList<EnemyUnitSpawnData> Enemies => enemies;
    }
}