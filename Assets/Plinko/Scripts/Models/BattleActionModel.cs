using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class BattleActionModel
    {
        public string ActionType;
        public int SourceRuntimeId;
        public int TargetRuntimeId;
        public int Value;
    }
}