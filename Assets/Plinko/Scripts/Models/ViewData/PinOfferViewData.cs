using System;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class PinOfferViewData
    {
        public int OfferId;
        public string PinTypeId;
        public string DisplayName;
        public int Price;
    }
}