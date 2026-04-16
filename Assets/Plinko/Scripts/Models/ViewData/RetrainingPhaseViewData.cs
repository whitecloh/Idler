using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class RetrainingPhaseViewData
    {
        public int SelectedCount;
        public int SelectionLimit;
        public bool CanConfirm;
        public bool IsSelectionLocked;
        public int ActiveTrainingCount;
        public List<OwnedUnitViewData> OwnedUnits = new();
    }
}