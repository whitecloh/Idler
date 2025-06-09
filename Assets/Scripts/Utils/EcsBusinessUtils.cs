using Game.Components;
using Game.Data.Business;
using Leopotam.EcsLite;

namespace Utils
{
    public static class EcsBusinessUtils
    {
        public static float CalculateTotalUpgradeMultiplier(EcsWorld world, EcsPool<UpgradeComponent> upgradePool, BusinessId businessId)
        {
            var totalMultiplier = 0f;
            foreach (var entity in world.Filter<UpgradeComponent>().End())
            {
                ref var upg = ref upgradePool.Get(entity);
                if (upg.BusinessId == businessId && upg.IsActive)
                    totalMultiplier += upg.Multiplier;
            }
            return totalMultiplier;
        }
    }
}