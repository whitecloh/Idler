using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class EnemyWaveModel
    {
        public int ThresholdPercent;
        public List<EnemyUnitSnapshotModel> Enemies = new();
        public int TotalAttack;
        public int TotalHealth;
    }
}