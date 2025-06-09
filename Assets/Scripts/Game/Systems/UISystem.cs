using Game.Components;
using Game.Services;
using Leopotam.EcsLite;
using UI;

namespace Game.Systems
{
    public class UISystem : IEcsRunSystem, IEcsInitSystem
    {
        private EcsFilter _balanceFilter;
        private EcsFilter _businessFilter;
        private EcsFilter _upgradeFilter;

        private EcsPool<BalanceComponent> _balancePool;
        private EcsPool<BusinessComponent> _businessPool;
        private EcsPool<IncomeProgressComponent> _progressPool;
        private EcsPool<IncomeComponent> _incomePool;
        private EcsPool<UpgradeComponent> _upgradePool;

        public void Init(IEcsSystems systems)
        {
            var world = systems.GetWorld();

            _balanceFilter = world.Filter<BalanceComponent>().End();
            _businessFilter = world.Filter<BusinessComponent>().Inc<IncomeProgressComponent>().Inc<IncomeComponent>().End();
            _upgradeFilter = world.Filter<UpgradeComponent>().End();

            _balancePool = world.GetPool<BalanceComponent>();
            _businessPool = world.GetPool<BusinessComponent>();
            _progressPool = world.GetPool<IncomeProgressComponent>();
            _incomePool = world.GetPool<IncomeComponent>();
            _upgradePool = world.GetPool<UpgradeComponent>();
        }

        public void Run(IEcsSystems systems)
        {
            var config = ConfigService.Instance;
            var playerBalance = 0l;

            foreach (var entity in _balanceFilter)
            {
                ref var balance = ref _balancePool.Get(entity);
                playerBalance = balance.Value;
                HUDController.Instance.SetBalance(balance.Value);
            }

            foreach (var bizId in config.GetAllBusinessIds())
            {
                var bizEntity = -1;
                foreach (var e in _businessFilter)
                {
                    ref var biz = ref _businessPool.Get(e);
                    if (biz.BusinessId == bizId)
                    {
                        bizEntity = e;
                        break;
                    }
                }

                var level = 0;
                var progress = 0f;
                var income = 0l;
                var isUnlocked = false;
                var upgrades = config.GetUpgradeConfigs(bizId);
                var upgradesBought = new bool[upgrades.Count];
                var canBuyUpgrade = new bool[upgrades.Count];

                if (bizEntity >= 0)
                {
                    ref var biz = ref _businessPool.Get(bizEntity);
                    ref var prog = ref _progressPool.Get(bizEntity);
                    ref var inc = ref _incomePool.Get(bizEntity);

                    level = biz.Level;
                    progress = prog.Progress;
                    isUnlocked = biz.Level > 0;
                    income = isUnlocked ? inc.Value : 0;

                    foreach (var upgEntity in _upgradeFilter)
                    {
                        ref var upg = ref _upgradePool.Get(upgEntity);
                        if (upg.BusinessId == biz.BusinessId &&
                            upg.Index >= 0 && upg.Index < upgrades.Count)
                        {
                            upgradesBought[upg.Index] = upg.IsActive;
                            canBuyUpgrade[upg.Index] = isUnlocked &&
                                (playerBalance >= config.GetUpgradePrice(biz.BusinessId, upg.Index)) &&
                                !upg.IsActive;
                        }
                    }
                }

                var buyLevelPrice = config.GetLevelPrice(bizId, level + 1);
                var canBuyLevel = playerBalance >= buyLevelPrice;

                if (!isUnlocked)
                {
                    for (int i = 0; i < canBuyUpgrade.Length; i++)
                        canBuyUpgrade[i] = false;

                    income = 0;
                    progress = 0f;
                }

                HUDController.Instance.UpdateBusinessPanel(
                    bizId,
                    level,
                    income,
                    progress,
                    isUnlocked,
                    buyLevelPrice,
                    upgrades,
                    upgradesBought,
                    canBuyLevel,
                    canBuyUpgrade
                );
            }
        }
    }
}