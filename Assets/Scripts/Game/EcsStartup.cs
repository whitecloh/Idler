using Utils;

namespace Game
{
    using Components;
    using Events;
    using Save;
    using Services;
    using Systems;
    using Leopotam.EcsLite;
    using UI;
    using UnityEngine;
    
    public class EcsStartup : MonoBehaviour
    {
        [Header("Dependencies")] 
        [SerializeField] private ConfigService configService;
        [SerializeField] private HUDController hudController;
        [SerializeField] private EcsUIEventBridge ecsUiEventBridge;

        private SaveData _loadedSaveData;
        private IEcsSystems _systems;
        private EcsWorld _world;
        
        private BusinessIndex _businessIndex;

        private void Awake()
        {
            configService.Init();
            hudController.Init(configService, ecsUiEventBridge);
            
            _loadedSaveData = SaveService.Load(configService.GetStartBalance, configService.GetAllBusinessIds());

            _world = new EcsWorld();
            _systems = new EcsSystems(_world);
            _businessIndex = new BusinessIndex();

            ecsUiEventBridge.Init(_world);

            _systems
                .Add(new BuyLevelSystem(configService, _businessIndex))
                .Add(new UpgradeSystem(configService))
                .Add(new RecalculateBusinessIncomeSystem(configService, _businessIndex))
                .Add(new IncomeSystem())
                .Add(new SaveSystem())
                .Add(new UISyncSystem(hudController, configService, _businessIndex));

            InitializeEntitiesFromSave(_loadedSaveData);

            _systems.Init();

            EmitInitialUiEvents();
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

            foreach (var businessIds in configService.GetAllBusinessIds())
            {
                var businessSave = saveData.Businesses[businessIds];
                var businessEntity = _world.NewEntity();
                ref var business = ref _world.GetPool<BusinessComponent>().Add(businessEntity);
                business.BusinessId = businessIds;
                business.Level = businessSave.Level;
                
                _businessIndex.Register(businessEntity, businessIds);

                ref var progress = ref _world.GetPool<IncomeProgressComponent>().Add(businessEntity);
                progress.Progress = businessSave.Progress;
                progress.Delay = configService.GetIncomeDelay(businessIds);

                var upgradeConfigs = configService.GetUpgradeConfigs(businessIds);
                for (var i = 0; i < upgradeConfigs.Count; i++)
                {
                    var upgradeEntity = _world.NewEntity();
                    ref var upgrade = ref _world.GetPool<UpgradeComponent>().Add(upgradeEntity);
                    upgrade.BusinessId = businessIds;
                    upgrade.Index = i;
                    upgrade.IsActive = businessSave.Upgrades[i].IsActive;
                    upgrade.Multiplier = upgradeConfigs[i].IncomeMultiplier;
                }

                var recalculateEventEntity = _world.NewEntity();
                ref var recalculateEvent = ref _world.GetPool<RecalculateIncomeEvent>().Add(recalculateEventEntity);
                recalculateEvent.BusinessId = businessIds;
            }
        }

        private void ForceSave()
        {
            if (_systems == null)
                return;

            var saveEntity = _world.NewEntity();
            _world.GetPool<SaveEvent>().Add(saveEntity);
            _systems.Run();
        }

        private void EmitInitialUiEvents()
        {
            var balanceChangeEntity = _world.NewEntity();
            _world.GetPool<BalanceChangedEvent>().Add(balanceChangeEntity);
            foreach (var businessId in configService.GetAllBusinessIds())
            {
                var entity = _world.NewEntity();
                ref var business = ref _world.GetPool<BusinessStateChangedEvent>().Add(entity);
                business.BusinessId = businessId;
            }

            _systems.Run();
        }
    }
}