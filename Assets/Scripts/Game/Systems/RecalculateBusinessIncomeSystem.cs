using Game.Components;
using Game.Events;
using Game.Services;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Systems
{
    public class RecalculateBusinessIncomeSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var recalcFilter = world.Filter<RecalculateIncomeEvent>().End();
            var businessPool = world.GetPool<BusinessComponent>();
            var upgradePool = world.GetPool<UpgradeComponent>();
            var currentIncomePool = world.GetPool<IncomeComponent>();

            foreach (var eventEntity in recalcFilter)
            {
                ref var recalcEvent = ref world.GetPool<RecalculateIncomeEvent>().Get(eventEntity);
                
                foreach (var bizEntity in world.Filter<BusinessComponent>().End())
                {
                    ref var biz = ref businessPool.Get(bizEntity);
                    if (biz.BusinessId != recalcEvent.BusinessId || biz.Level <= 0)
                        continue;
                    
                    var totalMultiplier = 0f;
                    foreach (var upgEntity in world.Filter<UpgradeComponent>().End())
                    {
                        ref var upg = ref upgradePool.Get(upgEntity);
                        if (upg.BusinessId == biz.BusinessId && upg.IsActive)
                            totalMultiplier += upg.Multiplier;
                    }

                    var baseIncome = ConfigService.Instance.GetBaseIncome(biz.BusinessId);
                    var income = Mathf.RoundToInt(biz.Level * baseIncome * (1f + totalMultiplier));

                    if (currentIncomePool.Has(bizEntity))
                        currentIncomePool.Get(bizEntity).Value = income;
                    else
                        currentIncomePool.Add(bizEntity).Value = income;
                }
                
                world.DelEntity(eventEntity);
            }
        }
    }
}