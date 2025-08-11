using Utils;

namespace Game.Systems
{
    using Components;
    using Events;
    using Services;
    using Leopotam.EcsLite;
    
    public sealed class BuyLevelSystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly ConfigService _config;
        private readonly BusinessIndex _index;
        
        private EcsFilter _buyEventFilter;
        private EcsPool<BuyLevelEvent> _buyEventPool;
        
        private EcsPool<BusinessComponent> _businessPool;

        private EcsFilter _balanceFilter;
        private EcsPool<BalanceComponent> _balancePool;

        private EcsPool<RecalculateIncomeEvent> _recalcPool;
        private EcsPool<BalanceChangedEvent> _balanceChangedPool;
        private EcsPool<BusinessStateChangedEvent> _bizChangedPool;
        
        private int _balanceEntity = -1;
        
        public BuyLevelSystem(ConfigService config, BusinessIndex index)
        {
            _config = config;
            _index = index;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            _buyEventFilter = world.Filter<BuyLevelEvent>().End();
            _balanceFilter = world.Filter<BalanceComponent>().End();

            _buyEventPool = world.GetPool<BuyLevelEvent>();
            _businessPool = world.GetPool<BusinessComponent>();
            _balancePool = world.GetPool<BalanceComponent>();
            _recalcPool = world.GetPool<RecalculateIncomeEvent>();
            _balanceChangedPool = world.GetPool<BalanceChangedEvent>();
            _bizChangedPool = world.GetPool<BusinessStateChangedEvent>();

            foreach (var entity in _balanceFilter)
            {
                _balanceEntity = entity;
                break;
            }
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (_balanceEntity < 0)
                return;

            foreach (var entity in _buyEventFilter)
            {
                ref var buy = ref _buyEventPool.Get(entity);

                if (!_index.TryGet(buy.BusinessId, out var businessEntity))
                {
                    world.DelEntity(entity);
                    continue;
                }

                ref var business = ref _businessPool.Get(businessEntity);
                var price = _config.GetLevelPrice(business.BusinessId, business.Level);

                ref var balance = ref _balancePool.Get(_balanceEntity);
                if (balance.Value >= price)
                {
                    balance.Value -= price;
                    business.Level++;

                    _recalcPool.Add(world.NewEntity()).BusinessId = business.BusinessId;
                    _balanceChangedPool.Add(world.NewEntity());
                    _bizChangedPool.Add(world.NewEntity()).BusinessId = business.BusinessId;
                }

                world.DelEntity(entity);
            }
        }
    }
}