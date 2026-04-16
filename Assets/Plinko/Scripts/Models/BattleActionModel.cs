using System;
using UnityEngine;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BattleActionModel
    {
        public int Tick;
        public string ActionType;
        public int SourceRuntimeId;
        public int TargetRuntimeId;
        public int Value;
        public Vector2Int TargetPosition;
    }
}