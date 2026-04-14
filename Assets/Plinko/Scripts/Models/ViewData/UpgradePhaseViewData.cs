using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class UpgradePhaseViewData
    {
        public int SelectedCount;
        public int SelectionLimit;
        public bool CanConfirm;
        public bool IsSelectionLocked;
        public List<OwnedUnitViewData> OwnedUnits = new();
    }
}