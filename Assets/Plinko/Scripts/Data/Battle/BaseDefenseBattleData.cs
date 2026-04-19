using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Enemies;
using UnityEngine;

namespace Plinko.Scripts.Data.Battle
{
    [Serializable]
    public sealed class BaseDefenseBattleData
    {
        [SerializeField] private int startingMana = 1;
        [SerializeField] private int maxMana = 10;
        [SerializeField] private int laneCount = 4;
        [SerializeField] private int cellsPerLane = 6;
        [SerializeField] private int playerSideCellCount = 3;
        [SerializeField] private List<BaseDefenseWaveData> waves = new();

        public int StartingMana => startingMana;
        public int MaxMana => maxMana;
        public int LaneCount => laneCount;
        public int CellsPerLane => cellsPerLane;
        public int PlayerSideCellCount => playerSideCellCount;
        public IReadOnlyList<BaseDefenseWaveData> Waves => waves;
        public int RequiredTurnCount => waves != null ? waves.Count : 0;
    }

    [Serializable]
    public sealed class BaseDefenseWaveData
    {
        [SerializeField] private int turnIndex = 1;
        [SerializeField] private List<BaseDefenseWaveSpawnData> spawns = new();

        public int TurnIndex => turnIndex;
        public IReadOnlyList<BaseDefenseWaveSpawnData> Spawns => spawns;
    }

    [Serializable]
    public sealed class BaseDefenseWaveSpawnData
    {
        [SerializeField] private EnemyUnitSpawnData enemy;
        [SerializeField] private int laneIndex;
        [SerializeField] private int enemySideCellIndex;

        public EnemyUnitSpawnData Enemy => enemy;
        public int LaneIndex => laneIndex;
        public int EnemySideCellIndex => enemySideCellIndex;
    }
}
