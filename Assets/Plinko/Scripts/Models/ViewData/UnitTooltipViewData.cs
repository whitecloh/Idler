using System;
using System.Collections.Generic;
using UnityEngine;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class UnitTooltipViewData
    {
        public string DisplayName;
        public Sprite PortraitSprite;
        public int ManaCost;
        public List<StatDisplayViewData> Stats = new();
    }
}
