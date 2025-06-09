using Game.Components;
using Game.Events;
using Game.Save;
using Game.Services;
using Game.Systems;
using Leopotam.EcsLite;
using UI;
using UnityEngine;

namespace Game
{
    public class EcsStartup : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private ConfigService configService;
        [SerializeField] private HUDController hudController;
        [SerializeField] private EcsUiEventBridge ecsUiEventBridge;
        
        private SaveData _loadedSaveData;
        private EcsWorld _world;
        private IEcsSystems _systems;

        private void Awake()
        {
            configService.Init();
            hudController.Init();
            
            _loadedSaveData = SaveService.Load();

            _world = new EcsWorld();
            _systems = new EcsSystems(_world);

            ecsUiEventBridge.Init(_world);
            
            var autosaveSystem = new AutosaveSystem();
            autosaveSystem.Init(configService.GetBaseAutoSaveInterval);
            
            _systems
                .Add(new RecalculateBusinessIncomeSystem())
                .Add(new IncomeSystem())
                .Add(new BuyLevelSystem())
                .Add(new UpgradeSystem())
                .Add(new SaveSystem())
                .Add(new UISystem())
                .Add(new OfflineIncomeSystem())
                .Add(autosaveSystem);

            InitializeEntitiesFromSave(_loadedSaveData);

            _systems.Init();
        }

        private void Update()
        {
            _systems?.Run();
        }

        private void OnDestroy()
        {
            _systems?.Destroy();
            _systems = null;
            _world?.Destroy();
            _world = null;
        }
        
        private void OnApplicationPause(bool pause)
        {
            if (pause)
                ForceSave();
        }

        private void OnApplicationQuit()
        {
            ForceSave();
        }
        
        private void InitializeEntitiesFromSave(SaveData saveData)
        {
            var playerEntity = _world.NewEntity();
            ref var balance = ref _world.GetPool<BalanceComponent>().Add(playerEntity);
            balance.Value = saveData.Balance;
            
            foreach (var bizId in configService.GetAllBusinessIds())
            {
                var bizSave = saveData.Businesses[bizId];
                var businessEntity = _world.NewEntity();
                ref var business = ref _world.GetPool<BusinessComponent>().Add(businessEntity);
                business.BusinessId = bizId;
                business.Level = bizSave.Level;

                ref var progress = ref _world.GetPool<IncomeProgressComponent>().Add(businessEntity);
                progress.Progress = bizSave.Progress;
                progress.Delay = configService.GetIncomeDelay(bizId);

                var upgradeConfigs = configService.GetUpgradeConfigs(bizId);
                for (int i = 0; i < upgradeConfigs.Count; i++)
                {
                    var upgradeEntity = _world.NewEntity();
                    ref var upgrade = ref _world.GetPool<UpgradeComponent>().Add(upgradeEntity);
                    upgrade.BusinessId = bizId;
                    upgrade.Index = i;
                    upgrade.IsActive = bizSave.Upgrades[i].IsActive;
                    upgrade.Multiplier = upgradeConfigs[i].IncomeMultiplier;
                }
                
                var recalcEventEntity = _world.NewEntity();
                ref var recalcEvent = ref _world.GetPool<RecalculateIncomeEvent>().Add(recalcEventEntity);
                recalcEvent.BusinessId = bizId;
            }
            
            var appStateEntity = _world.NewEntity();
            ref var appState = ref _world.GetPool<OfflineIncomeComponent>().Add(appStateEntity);
            appState.LastSaveTimestamp = saveData.LastSaveTimestamp.Value;
        }

        private void ForceSave()
        {
            if (_systems == null) 
                return;
            
            var saveEntity = _world.NewEntity();
            _world.GetPool<SaveEvent>().Add(saveEntity);
            _systems.Run();
        }
    }
}