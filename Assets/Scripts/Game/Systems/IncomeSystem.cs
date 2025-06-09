using Game.Components;
using Leopotam.EcsLite;
using UnityEngine;

namespace Game.Systems
{
    public class IncomeSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsFilter _businessFilter;
        private EcsFilter _balanceFilter;

        private EcsPool<BusinessComponent> _businessPool;
        private EcsPool<IncomeProgressComponent> _progressPool;
        private EcsPool<IncomeComponent> _incomePool;
        private EcsPool<BalanceComponent> _balancePool;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            _businessFilter = world.Filter<BusinessComponent>()
                .Inc<IncomeProgressComponent>()
                .Inc<IncomeComponent>()
                .End();

            _balanceFilter = world.Filter<BalanceComponent>().End();

            _businessPool = world.GetPool<BusinessComponent>();
            _progressPool = world.GetPool<IncomeProgressComponent>();
            _incomePool = world.GetPool<IncomeComponent>();
            _balancePool = world.GetPool<BalanceComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            foreach (var entity in _businessFilter)
            {
                ref var business = ref _businessPool.Get(entity);
                ref var progress = ref _progressPool.Get(entity);

                if (business.Level <= 0)
                    continue;

                progress.Progress += Time.deltaTime / progress.Delay;
                if (progress.Progress >= 1f)
                {
                    progress.Progress = 0f;

                    var income = _incomePool.Get(entity).Value;

                    foreach (var balanceEntity in _balanceFilter)
                    {
                        ref var balance = ref _balancePool.Get(balanceEntity);
                        balance.Value += income;
                    }
                }
            }
        }
    }
}