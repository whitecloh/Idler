using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class FieldUpgradePhaseViewData
    {
        public int Gold;
        public int RerollCount;
        public int RerollPrice;
        public bool CanReroll;
        public bool HasPendingPin;
        public string PendingPinTypeId;
        public int SelectedSlotIndex;
        public bool CanReplace;
        public List<PinOfferViewData> Offers = new();
        public List<BoardSlotViewData> Slots = new();
    }
}