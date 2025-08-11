namespace Game.Systems
{
    using Components;
    using Data.Business;
    using Events;
    using Services;
    using Leopotam.EcsLite;
    
    public sealed class UpgradeSystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly ConfigService _config;

        private EcsFilter _upgradeEventFilter;
        private EcsPool<UpgradeEvent> _upgradeEventPool;
        
        private EcsPool<UpgradeComponent> _upgradePool;

        private EcsFilter _balanceFilter;
        private EcsPool<BalanceComponent> _balancePool;

        private EcsPool<RecalculateIncomeEvent> _recalcPool;
        private EcsPool<BalanceChangedEvent> _balanceChangedPool;
        private EcsPool<BusinessStateChangedEvent> _bizChangedPool;

        private int _balanceEntity = -1;

        public UpgradeSystem(ConfigService config)
        {
            _config = config;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            _upgradeEventFilter = world.Filter<UpgradeEvent>().End();
            _balanceFilter = world.Filter<BalanceComponent>().End();

            _upgradeEventPool = world.GetPool<UpgradeEvent>();
            _upgradePool = world.GetPool<UpgradeComponent>();
            _balancePool = world.GetPool<BalanceComponent>();

            _recalcPool = world.GetPool<RecalculateIncomeEvent>();
            _balanceChangedPool = world.GetPool<BalanceChangedEvent>();
            _bizChangedPool = world.GetPool<BusinessStateChangedEvent>();

            foreach (var balanceEntity in _balanceFilter)
            {
                _balanceEntity = balanceEntity;
                break;
            }
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (_balanceEntity < 0) return;

            foreach (var entity in _upgradeEventFilter)
            {
                ref var upgradeEvent = ref _upgradeEventPool.Get(entity);

                var upgradeEntity = FindUpgradeEntity(upgradeEvent.BusinessId, upgradeEvent.UpgradeIndex);
                if (upgradeEntity >= 0)
                {
                    ref var upgrade = ref _upgradePool.Get(upgradeEntity);
                    if (!upgrade.IsActive)
                    {
                        var price = _config.GetUpgradePrice(upgrade.BusinessId, upgrade.Index);
                        ref var balance = ref _balancePool.Get(_balanceEntity);

                        if (balance.Value >= price)
                        {
                            balance.Value -= price;
                            upgrade.IsActive = true;

                            _recalcPool.Add(world.NewEntity()).BusinessId = upgrade.BusinessId;
                            _balanceChangedPool.Add(world.NewEntity());
                            _bizChangedPool.Add(world.NewEntity()).BusinessId = upgrade.BusinessId;
                        }
                    }
                }

                world.DelEntity(entity);
            }
        }

        private int FindUpgradeEntity(BusinessId id, int index)
        {
            var world = _upgradePool.GetWorld();
            var filter = world.Filter<UpgradeComponent>().End();
            foreach (var entity in filter)
            {
                ref var upgrade = ref _upgradePool.Get(entity);
                if (upgrade.BusinessId == id && upgrade.Index == index)
                    return entity;
            }
            return -1;
        }
    }
}