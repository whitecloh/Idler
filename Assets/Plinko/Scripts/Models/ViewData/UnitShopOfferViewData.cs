using System;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class UnitShopOfferViewData
    {
        public int OfferId;
        public string UnitTypeId;
        public string DisplayName;
        public Sprite PortraitSprite;
        public int Attack;
        public int Health;
        public int ManaCost;
        public int Price;
    }
}
