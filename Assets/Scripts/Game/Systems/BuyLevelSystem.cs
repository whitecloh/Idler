using Game.Components;
using Game.Events;
using Game.Services;
using Leopotam.EcsLite;
namespace Game.Systems
{
    public class BuyLevelSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var buyEventFilter = world.Filter<BuyLevelEvent>().End();
            var businessPool = world.GetPool<BusinessComponent>();
            var balancePool = world.GetPool<BalanceComponent>();

            foreach (var eventEntity in buyEventFilter)
            {
                ref var buyEvent = ref world.GetPool<BuyLevelEvent>().Get(eventEntity);
                foreach (var bizEntity in world.Filter<BusinessComponent>().End())
                {
                    ref var biz = ref businessPool.Get(bizEntity);
                    if (biz.BusinessId != buyEvent.BusinessId)
                        continue;
                    
                    var price = ConfigService.Instance.GetLevelPrice(biz.BusinessId, biz.Level + 1);

                    foreach (var balEntity in world.Filter<BalanceComponent>().End())
                    {
                        ref var balance = ref balancePool.Get(balEntity);
                        if (balance.Value >= price)
                        {
                            balance.Value -= price;
                            biz.Level++;

                            var recalcEventEntity = world.NewEntity();
                            ref var recalcEvent = ref world.GetPool<RecalculateIncomeEvent>().Add(recalcEventEntity);
                            recalcEvent.BusinessId = biz.BusinessId;
                            
                            break; 
                        }
                    }
                }

                world.DelEntity(eventEntity);
            }
        }
    }
}