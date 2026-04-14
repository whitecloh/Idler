using System;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class UnitShopOfferViewData
    {
        public int OfferId;
        public string UnitTypeId;
        public string DisplayName;
        public int Price;
    }
}