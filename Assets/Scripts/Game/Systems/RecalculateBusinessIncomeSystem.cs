namespace Game.Systems
{
    using Components;
    using Events;
    using Services;
    using Leopotam.EcsLite;
    using UnityEngine;
    using Utils;
    
    public sealed class RecalculateBusinessIncomeSystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly ConfigService _config;
        private readonly BusinessIndex _index;

        private EcsFilter _recalcFilter;
        private EcsPool<RecalculateIncomeEvent> _recalcPool;
        
        private EcsPool<BusinessComponent> _businessPool;

        private EcsPool<UpgradeComponent> _upgradePool;
        private EcsPool<IncomeComponent> _incomePool;

        public RecalculateBusinessIncomeSystem(ConfigService config, BusinessIndex index)
        {
            _config = config;
            _index = index;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            _recalcFilter = world.Filter<RecalculateIncomeEvent>().End();

            _recalcPool = world.GetPool<RecalculateIncomeEvent>();
            _businessPool = world.GetPool<BusinessComponent>();
            _upgradePool = world.GetPool<UpgradeComponent>();
            _incomePool = world.GetPool<IncomeComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            foreach (var entity in _recalcFilter)
            {
                var businessId = _recalcPool.Get(entity).BusinessId;

                if (!_index.TryGet(businessId, out var businessEntity))
                {
                    world.DelEntity(entity);
                    continue;
                }

                ref var business = ref _businessPool.Get(businessEntity);
                if (business.Level > 0)
                {
                    var baseIncome = _config.GetBaseIncome(businessId);
                    var multiplier = EcsBusinessUtils.CalculateTotalUpgradeMultiplier(world, _upgradePool, businessId);
                    var rawIncome = business.Level * baseIncome * (1f + multiplier);
                    var income = (long)Mathf.Round(rawIncome);

                    if (_incomePool.Has(businessEntity))
                    {
                        _incomePool.Get(businessEntity).Value = income;
                    }
                    else
                    {
                        _incomePool.Add(businessEntity).Value = income;
                    }
                }

                world.DelEntity(entity);
            }
        }
    }
}