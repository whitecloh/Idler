using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class PlinkoPathNodeModel
    {
        public int RowIndex;
        public int ColumnIndex;
        public string PinTypeId;
        public int AttackDelta;
        public int HealthDelta;
        public int ManaDelta;
        public float MoveSpeedDelta;
        public int AttackRangeDelta;
        public float AttackSpeedDelta;
    }
}
