namespace Game.Systems
{
    using Components;
    using Events;
    using Save;
    using Services;
    using Leopotam.EcsLite;
    
    public sealed class SaveSystem : IEcsRunSystem
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
                ref var business = ref businessPool.Get(entity);
                ref var progress = ref progressPool.Get(entity);

                var bizSave = saveData.Businesses[business.BusinessId];
                bizSave.Level = business.Level;
                bizSave.Progress = progress.Progress;
            }
        }

        private void SaveUpgrades(EcsWorld world, SaveData saveData)
        {
            var upgradePool = world.GetPool<UpgradeComponent>();
            foreach (var upgradeEntity in world.Filter<UpgradeComponent>().End())
            {
                ref var upgrade = ref upgradePool.Get(upgradeEntity);
                var upgradeSave = saveData.Businesses[upgrade.BusinessId].Upgrades[upgrade.Index];
                upgradeSave.IsActive = upgrade.IsActive;
            }
        }
    }
}