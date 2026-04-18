using System;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class RetrainingOfferViewData
    {
        public int OfferSlotIndex;
        public int RuntimeId;
        public string DisplayName;
        public string UnitTypeId;
        public Sprite PortraitSprite;
        public int Level;
        public int Attack;
        public int Health;
        public int ManaCost;
        public int UpgradeCount;
        public int Price;
    }
}
