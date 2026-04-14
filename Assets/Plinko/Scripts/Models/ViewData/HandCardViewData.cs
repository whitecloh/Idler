using System;

namespace Plinko.Scripts.Models.ViewData
{
    [Serializable]
    public sealed class HandCardViewData
    {
        public int CardId;
        public int RuntimeId;
        public string DisplayName;
        public int Level;
        public string UnitTypeId;
        public int Attack;
        public int Health;
        public int ManaCost;
        public bool IsDeployed;
    }
}