namespace Game.Systems
{
    using Components;
    using Events;
    using Leopotam.EcsLite;
    using UnityEngine;

    public sealed class IncomeSystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsFilter _businessFilter;
        private EcsPool<BusinessComponent> _businessPool;

        private EcsPool<IncomeComponent> _incomePool;
        private EcsPool<IncomeProgressComponent> _progressPool;

        private EcsFilter _balanceFilter;
        private EcsPool<BalanceComponent> _balancePool;

        private EcsPool<BalanceChangedEvent> _balanceChangedPool;
        private EcsPool<BusinessStateChangedEvent> _businessChangedPool;

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
            _balanceChangedPool = world.GetPool<BalanceChangedEvent>();
            _businessChangedPool = world.GetPool<BusinessStateChangedEvent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var entity in _businessFilter)
            {
                ref var business = ref _businessPool.Get(entity);
                ref var progress = ref _progressPool.Get(entity);

                if (business.Level <= 0) continue;
                if (progress.Delay <= 0.0001f) continue;

                progress.Progress += Time.deltaTime / progress.Delay;
                if (progress.Progress >= 1f)
                {
                    progress.Progress = 0f;

                    var income = _incomePool.Get(entity).Value;

                    foreach (var balanceEntity in _balanceFilter)
                    {
                        ref var balance = ref _balancePool.Get(balanceEntity);
                        balance.Value += income;

                        _balanceChangedPool.Add(world.NewEntity());
                        _businessChangedPool.Add(world.NewEntity()).BusinessId = business.BusinessId;
                    }
                }
            }
        }
    }
}