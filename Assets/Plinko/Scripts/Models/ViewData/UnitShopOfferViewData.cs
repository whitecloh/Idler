using System;
using System.Collections.Generic;
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
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
        public int Price;
        public List<StatDisplayViewData> Stats = new();
    }
}
