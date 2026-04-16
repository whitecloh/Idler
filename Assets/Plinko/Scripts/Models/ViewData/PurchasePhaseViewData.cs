using System;
using System.Collections.Generic;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class PurchasePhaseViewData
    {
        public int Gold;
        public int RerollCount;
        public int RerollPrice;
        public bool CanReroll;
        public bool CanStartBattle;
        public int ActiveTrainingCount;
        public List<UnitShopOfferViewData> Offers = new();
    }
}