using Game.Components;
using Game.Events;
using Game.Save;
using Game.Services;
using Leopotam.EcsLite;

namespace Game.Systems
{
    public class SaveSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            var saveEventFilter = world.Filter<SaveEvent>().End();

            if (saveEventFilter.GetEntitiesCount() <= 0) 
                return;

            var saveData = new SaveData();
            
            var balanceFilter = world.Filter<BalanceComponent>().End();
            var balancePool = world.GetPool<BalanceComponent>();
            foreach (var entity in balanceFilter)
            {
                ref var balance = ref balancePool.Get(entity);
                saveData.Balance = balance.Value;
            }
            
            var businessFilter = world.Filter<BusinessComponent>().Inc<IncomeProgressComponent>().End();
            var businessPool = world.GetPool<BusinessComponent>();
            var progressPool = world.GetPool<IncomeProgressComponent>();
            var upgradePool = world.GetPool<UpgradeComponent>();

            foreach (var entity in businessFilter)
            {
                ref var biz = ref businessPool.Get(entity);
                ref var progress = ref progressPool.Get(entity);

                var bizSave = saveData.Businesses[biz.BusinessId];
                bizSave.Level = biz.Level;
                bizSave.Progress = progress.Progress;

                var upgradeConfigs = ConfigService.Instance.GetUpgradeConfigs(biz.BusinessId);
                
                for (int i = 0; i < upgradeConfigs.Count; i++)
                {
                    foreach (var upgEntity in world.Filter<UpgradeComponent>().End())
                    {
                        ref var upg = ref upgradePool.Get(upgEntity);
                        if (upg.BusinessId == biz.BusinessId && upg.Index == i)
                        {
                            bizSave.Upgrades[i].IsActive = upg.IsActive;
                        }
                    }
                }
            }

            SaveService.Save(saveData);

            foreach (var entity in saveEventFilter)
                world.DelEntity(entity);
        }
    }
}