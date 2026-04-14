using System;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BoardSlotViewData
    {
        public int GlobalIndex;
        public int RowIndex;
        public int ColumnIndex;
        public string PinTypeId;
        public string DisplayName;
        public bool IsSelected;
    }
}