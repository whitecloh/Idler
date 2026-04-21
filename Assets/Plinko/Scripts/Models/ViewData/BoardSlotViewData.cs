using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class BoardSlotViewData
    {
        public int SlotIndex;
        public int RowIndex;
        public int ColumnIndex;
        public string PinTypeId;
        public string DisplayName;
        public string TooltipText;
        public Sprite Sprite;
        public List<StatDisplayViewData> ModifierLines = new();
        public bool IsSelected;
        public bool IsPlacementHighlighted;
        public bool IsAvailableForReplacement;
        public bool IsSelectedForReplacement;
        public bool IsNotSelectedForReplacement;
    }
}
