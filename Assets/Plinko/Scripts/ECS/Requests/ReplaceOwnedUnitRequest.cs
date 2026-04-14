namespace Plinko.Scripts.ECS.Requests
{
    public struct ReplaceOwnedUnitRequest
    {
        public int RuntimeId;
        public string DisplayName;
        public int Level;
        public string UnitTypeId;
        public int Attack;
        public int Health;
        public int ManaCost;
        public string PassiveAbilityId;
        public int UpgradeCount;
    }
}