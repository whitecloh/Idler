using System;

namespace Plinko.Scripts.Models
{
    [Serializable]
    public sealed class OwnedUnitSaveDto
    {
        public int RuntimeId;
        public string DisplayName;
        public int Level;
        public string UnitTypeId;
        public int Attack;
        public int Health;
        public int ManaCost;
        public float MoveSpeed;
        public int AttackRange;
        public float AttackSpeed;
        public string PassiveAbilityId;
        public int UpgradeCount;
    }
}
