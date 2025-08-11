namespace Utils
{
    using Game.Components;
    using Game.Data.Business;
    using Leopotam.EcsLite;
    
    public static class EcsBusinessUtils
    {
        public static float CalculateTotalUpgradeMultiplier(EcsWorld world, EcsPool<UpgradeComponent> upgradePool,
            BusinessId businessId)
        {
            var totalMultiplier = 0f;
            foreach (var entity in world.Filter<UpgradeComponent>().End())
            {
                ref var upgrade = ref upgradePool.Get(entity);
                if (upgrade.BusinessId == businessId && upgrade.IsActive)
                {
                    totalMultiplier += upgrade.Multiplier;
                }
            }

            return totalMultiplier;
        }
    }
}