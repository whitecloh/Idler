using Game.Components;
using Game.Events;
using Game.Services;
using Leopotam.EcsLite;
using UnityEngine;
using Utils;

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
            var incomePool = world.GetPool<IncomeComponent>();

            foreach (var eventEntity in recalcFilter)
            {
                ref var recalcEvent = ref world.GetPool<RecalculateIncomeEvent>().Get(eventEntity);
                var businessId = recalcEvent.BusinessId;

                foreach (var bizEntity in world.Filter<BusinessComponent>().End())
                {
                    ref var biz = ref businessPool.Get(bizEntity);
                    if (biz.BusinessId != businessId || biz.Level <= 0)
                        continue;

                    var baseIncome = ConfigService.Instance.GetBaseIncome(businessId);
                    var multiplier = EcsBusinessUtils.CalculateTotalUpgradeMultiplier(world, upgradePool, businessId);
                    var rawIncome = biz.Level * baseIncome * (1f + multiplier);
                    var income = (long)Mathf.Round(rawIncome);

                    if (incomePool.Has(bizEntity))
                        incomePool.Get(bizEntity).Value = income;
                    else
                        incomePool.Add(bizEntity).Value = income;
                }

                world.DelEntity(eventEntity);
            }
        }
    }
}