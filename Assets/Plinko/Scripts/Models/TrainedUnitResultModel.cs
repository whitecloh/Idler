using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class TrainedUnitResultModel
    {
        public int RuntimeId;
        public string UnitTypeId;
        public string DisplayName;
        public int Level;
        public int FinalAttack;
        public int FinalHealth;
        public int FinalManaCost;
        public string PassiveAbilityId;
        public int UpgradeCount;
        public string BasketId;
    }
}