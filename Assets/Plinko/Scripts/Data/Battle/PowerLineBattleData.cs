using System;
using System.Collections.Generic;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Data.Enemies;
using UnityEngine;

namespace Plinko.Scripts.Data.Battle
{
    [Serializable]
    public sealed class PowerLineBattleData
    {
        [SerializeField] private float laneLength = 12f;
        [SerializeField] private List<PowerLineSpawnEntryData> spawns = new();

        public float LaneLength => laneLength;
        public IReadOnlyList<PowerLineSpawnEntryData> Spawns => spawns;
        public int RequiredConnectedLineCount => 4;
    }

    [Serializable]
    public sealed class PowerLineSpawnEntryData
    {
        [SerializeField] private int timeTick;
        [SerializeField] private EnemyUnitSpawnData enemy;
        [SerializeField] private Enums.PowerLineLane lane;

        public int TimeTick => timeTick;
        public EnemyUnitSpawnData Enemy => enemy;
        public Enums.PowerLineLane Lane => lane;
    }
}
