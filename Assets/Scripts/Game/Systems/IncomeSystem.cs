using Game.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Systems
{
    public class IncomeSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            var businessFilter = world.Filter<BusinessComponent>()
                .Inc<IncomeProgressComponent>()
                .Inc<IncomeComponent>()
                .End();

            var businessPool = world.GetPool<BusinessComponent>();
            var progressPool = world.GetPool<IncomeProgressComponent>();
            var incomePool = world.GetPool<IncomeComponent>();
            var balanceFilter = world.Filter<BalanceComponent>().End();
            var balancePool = world.GetPool<BalanceComponent>();

            foreach (var entity in businessFilter)
            {
                ref var business = ref businessPool.Get(entity);
                ref var progress = ref progressPool.Get(entity);

                if (business.Level <= 0)
                    continue;

                progress.Progress += Time.deltaTime / progress.Delay;
                if (progress.Progress >= 1f)
                {
                    progress.Progress = 0f;

                    int income = incomePool.Get(entity).Value;

                    foreach (var balanceEntity in balanceFilter)
                    {
                        ref var balance = ref balancePool.Get(balanceEntity);
                        balance.Value += income;
                    }
                }
            }
        }
    }
}