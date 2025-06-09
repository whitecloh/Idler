using Game.Components;
using Game.Events;
using Game.Services;
using Leopotam.EcsLite;

namespace Game.Systems
{
    public class UpgradeSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var upgradeEventFilter = world.Filter<UpgradeEvent>().End();
            var upgradePool = world.GetPool<UpgradeComponent>();
            var balancePool = world.GetPool<BalanceComponent>();

            foreach (var eventEntity in upgradeEventFilter)
            {
                ref var upgradeEvent = ref world.GetPool<UpgradeEvent>().Get(eventEntity);

                foreach (var upgEntity in world.Filter<UpgradeComponent>().End())
                {
                    ref var upg = ref upgradePool.Get(upgEntity);
                    if (upg.BusinessId == upgradeEvent.BusinessId && upg.Index == upgradeEvent.UpgradeIndex && !upg.IsActive)
                    {
                        var price = ConfigService.Instance.GetUpgradePrice(upg.BusinessId, upg.Index);

                        foreach (var balEntity in world.Filter<BalanceComponent>().End())
                        {
                            ref var balance = ref balancePool.Get(balEntity);
                            if (balance.Value >= price)
                            {
                                balance.Value -= price;
                                upg.IsActive = true;                       

                                var recalcEventEntity = world.NewEntity();
                                ref var recalcEvent = ref world.GetPool<RecalculateIncomeEvent>().Add(recalcEventEntity);
                                recalcEvent.BusinessId = upg.BusinessId;
                            }
                        }
                    }
                }
                world.DelEntity(eventEntity);
            }
        }
    }
}