using Game.Components;
using Game.Services;
using Leopotam.EcsLite;
using UI;

namespace Game.Systems
{
    public class UISystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var configService = ConfigService.Instance;
            var balanceFilter = world.Filter<BalanceComponent>().End();
            var balancePool = world.GetPool<BalanceComponent>();
            var businessFilter = world.Filter<BusinessComponent>().Inc<IncomeProgressComponent>().Inc<IncomeComponent>().End();
            var businessPool = world.GetPool<BusinessComponent>();
            var progressPool = world.GetPool<IncomeProgressComponent>();
            var incomePool = world.GetPool<IncomeComponent>();
            var upgradePool = world.GetPool<UpgradeComponent>();

            var playerBalance = 0;
            foreach (var entity in balanceFilter)
            {
                ref var balance = ref balancePool.Get(entity);
                playerBalance = balance.Value;
                HUDController.Instance.SetBalance(balance.Value);
            }
            
            foreach (var bizId in configService.GetAllBusinessIds())
            {
                var bizEntity = -1;
                foreach (var e in businessFilter)
                {
                    ref var biz = ref businessPool.Get(e);
                    if (biz.BusinessId == bizId)
                    {
                        bizEntity = e;
                        break;
                    }
                }

                var level = 0;
                var progress = 0f;
                var income = 0;
                var isUnlocked = false;
                var upgrades = configService.GetUpgradeConfigs(bizId);
                var upgradesBought = new bool[upgrades.Count];
                var canBuyUpgrade = new bool[upgrades.Count];

                if (bizEntity >= 0)
                {
                    ref var biz = ref businessPool.Get(bizEntity);
                    ref var prog = ref progressPool.Get(bizEntity);
                    ref var inc = ref incomePool.Get(bizEntity);

                    level = biz.Level;
                    progress = prog.Progress;
                    isUnlocked = biz.Level > 0;
                    income = isUnlocked ? inc.Value : 0;

                    foreach (var upgEntity in world.Filter<UpgradeComponent>().End())
                    {
                        ref var upg = ref upgradePool.Get(upgEntity);
                        if (upg.BusinessId == biz.BusinessId && upg.Index >= 0 && upg.Index < upgrades.Count)
                        {
                            upgradesBought[upg.Index] = upg.IsActive;
                            canBuyUpgrade[upg.Index] = isUnlocked &&
                                (playerBalance >= configService.GetUpgradePrice(biz.BusinessId, upg.Index)) &&
                                !upg.IsActive;
                        }
                    }
                }

                var buyLevelPrice = configService.GetLevelPrice(bizId, level + 1);
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