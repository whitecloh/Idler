using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class PinOfferViewData
    {
        public int OfferId;
        public string PinTypeId;
        public string DisplayName;
        public Sprite Sprite;
        public int Price;
        public List<StatDisplayViewData> ModifierLines = new();
    }
}
