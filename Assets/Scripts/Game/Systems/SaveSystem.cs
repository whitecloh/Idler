using Leopotam.EcsLite;
using Game.Components;
using Game.Events;
using Game.Save;
using Game.Services;

namespace Game.Systems 
{ 
    public class SaveSystem : IEcsRunSystem
    {
        public void Run(IEcsSystems systems)
        {
            var world = systems.GetWorld();
            if (!HasSaveEvent(world))
                return;

            var saveData = new SaveData();

            SaveBalance(world, saveData);
            SaveBusinesses(world, saveData);
            SaveUpgrades(world, saveData);

            SaveService.Save(saveData);
            ClearSaveEvents(world);
        }

        private bool HasSaveEvent(EcsWorld world)
        {
            return world.Filter<SaveEvent>().End().GetEntitiesCount() > 0;
        }

        private void ClearSaveEvents(EcsWorld world)
        {
            var saveEventFilter = world.Filter<SaveEvent>().End();
            foreach (var entity in saveEventFilter)
                world.DelEntity(entity);
        }

        private void SaveBalance(EcsWorld world, SaveData saveData)
        {
            var balancePool = world.GetPool<BalanceComponent>();
            foreach (var entity in world.Filter<BalanceComponent>().End())
            {
                ref var balance = ref balancePool.Get(entity);
                saveData.Balance = balance.Value;
            }
        }

        private void SaveBusinesses(EcsWorld world, SaveData saveData)
        {
            var businessPool = world.GetPool<BusinessComponent>();
            var progressPool = world.GetPool<IncomeProgressComponent>();

            foreach (var entity in world.Filter<BusinessComponent>().Inc<IncomeProgressComponent>().End())
            {
                ref var biz = ref businessPool.Get(entity);
                ref var progress = ref progressPool.Get(entity);

                var bizSave = saveData.Businesses[biz.BusinessId];
                bizSave.Level = biz.Level;
                bizSave.Progress = progress.Progress;
            }
        }

        private void SaveUpgrades(EcsWorld world, SaveData saveData)
        {
            var upgradePool = world.GetPool<UpgradeComponent>();
            foreach (var upgEntity in world.Filter<UpgradeComponent>().End())
            {
                ref var upg = ref upgradePool.Get(upgEntity);
                var upgradeSave = saveData.Businesses[upg.BusinessId].Upgrades[upg.Index];
                upgradeSave.IsActive = upg.IsActive;
            }
        }
    }
}