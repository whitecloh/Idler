namespace Game.Systems
{
    using Components;
    using Data.Business;
    using Events;
    using Services;
    using Leopotam.EcsLite;
    using UI;
    using Utils;

    public sealed class UISyncSystem : IEcsRunSystem, IEcsInitSystem
    {
        private readonly HUDController _hud;
        private readonly ConfigService _config;
        private readonly BusinessIndex _index;

        private EcsFilter _balanceFilter;
        private EcsPool<BalanceComponent> _balancePool;
        private int _balanceEntity = -1;

        private EcsFilter _businessFilter;
        private EcsPool<BusinessComponent> _businessPool;

        private EcsFilter _progressFilter;
        private EcsPool<IncomeProgressComponent> _progressPool;

        private EcsPool<IncomeComponent> _incomePool;
        private EcsFilter _upgradeFilter;
        private EcsPool<UpgradeComponent> _upgradePool;

        private EcsFilter _balanceChangedFilter;
        private EcsFilter _businessChangedFilter;
        private EcsPool<BusinessStateChangedEvent> _businessChangedPool;

        public UISyncSystem(HUDController hud, ConfigService config, BusinessIndex index)
        {
            _hud = hud;
            _config = config;
            _index = index;
        }

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            _balanceFilter = world.Filter<BalanceComponent>().End();
            _businessFilter = world.Filter<BusinessComponent>().End();
            _progressFilter = world.Filter<BusinessComponent>().Inc<IncomeProgressComponent>().End();

            _balanceChangedFilter = world.Filter<BalanceChangedEvent>().End();
            _businessChangedFilter = world.Filter<BusinessStateChangedEvent>().End();

            _balancePool = world.GetPool<BalanceComponent>();
            _businessPool = world.GetPool<BusinessComponent>();
            _progressPool = world.GetPool<IncomeProgressComponent>();
            _incomePool = world.GetPool<IncomeComponent>();

            _upgradeFilter = world.Filter<UpgradeComponent>().End();
            _upgradePool = world.GetPool<UpgradeComponent>();
            
            _businessChangedPool = world.GetPool<BusinessStateChangedEvent>();

            foreach (var balanceEntity in _balanceFilter)
            {
                _balanceEntity = balanceEntity;
                break;
            }
        }

        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            var playerBalance = _balanceEntity >= 0 ? _balancePool.Get(_balanceEntity).Value : 0L;
            
            if (_balanceChangedFilter.GetEntitiesCount() > 0)
            {
                _hud.SetBalance(playerBalance);

                foreach (var businessEntity in _businessFilter)
                {
                    ref var b = ref _businessPool.Get(businessEntity);
                    UpdateBusinessStatic(b.BusinessId, playerBalance);
                }

                foreach (var balanceChangeEntity in _balanceChangedFilter)
                {
                    world.DelEntity(balanceChangeEntity);
                }
            }
            
            foreach (var businessChangeEntity in _businessChangedFilter)
            {
                var id = _businessChangedPool.Get(businessChangeEntity).BusinessId;
                UpdateBusinessStatic(id, playerBalance);
                world.DelEntity(businessChangeEntity);
            }
            
            foreach (var progressEntity in _progressFilter)
            {
                ref var business = ref _businessPool.Get(progressEntity);
                var fill = business.Level > 0 ? _progressPool.Get(progressEntity).Progress : 0f;
                _hud.UpdateBusinessPanelProgress(business.BusinessId, fill);
            }
        }

        private void UpdateBusinessStatic(BusinessId id, long playerBalance)
        {
            if (!_index.TryGet(id, out var entity)) 
                return;

            ref var business = ref _businessPool.Get(entity);
            var level = business.Level;
            var isUnlocked = level > 0;

            var levelPrice = _config.GetLevelPrice(id, level);

            var income = 0L;
            if (isUnlocked && _incomePool.Has(entity))
            {
                income = _incomePool.Get(entity).Value;
            }

            var upgrades = _config.GetUpgradeConfigs(id);
            var upgradesBought = new bool[upgrades.Count];
            var canBuyUpgrade = new bool[upgrades.Count];

            foreach (var upgradeEntity in _upgradeFilter)
            {
                ref var upgrade = ref _upgradePool.Get(upgradeEntity);
                if (upgrade.BusinessId == id && upgrade.Index >= 0 && upgrade.Index < upgrades.Count)
                {
                    upgradesBought[upgrade.Index] = upgrade.IsActive;
                    canBuyUpgrade[upgrade.Index] = isUnlocked && !upgrade.IsActive &&
                                                   playerBalance >= _config.GetUpgradePrice(id, upgrade.Index);
                }
            }

            var canBuyLevel = playerBalance >= levelPrice;
            if (!isUnlocked)
            {
                income = 0L;
            }

            _hud.UpdateBusinessPanelStatic(id, level, income, isUnlocked, levelPrice,
                upgrades, upgradesBought, canBuyLevel, canBuyUpgrade);
        }
    }
}