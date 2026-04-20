using System.Collections.Generic;
using System.IO;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Settings;
using Plinko.Scripts.Data.Stats;
using Plinko.Scripts.Data.Units;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;
using UnityEngine;

namespace Plinko.Scripts.Bootstrap
{
    public sealed class GameServicesInstaller : MonoBehaviour
    {
        [SerializeField] private GameSettingsData gameSettingsData;
        [SerializeField] private List<LocationData> locations = new();
        [SerializeField] private List<UnitTypeData> unitTypes = new();
        [SerializeField] private UnitNamesData unitNamesData;
        [SerializeField] private List<PinTypeData> pinTypes = new();
        [SerializeField] private List<StatTypeData> statTypes = new();

        public GameServicesContext Build()
        {
            var unlocksService = new UnlocksService();
            var metaSaveService = new MetaSaveService(Path.Combine(Application.persistentDataPath, "session_meta_save.json"));
            unlocksService.ImportProgress(metaSaveService.Load());
            var gameSettingsService = new GameSettingsService(gameSettingsData);
            var locationConfigService = new LocationConfigService(locations);
            var levelConfigService = new LevelConfigService(locationConfigService);
            var unitConfigService = new UnitConfigService(unitTypes, unlocksService);
            var pinConfigService = new PinConfigService(pinTypes, unlocksService);
            var statTypeConfigService = new StatTypeConfigService(statTypes);
            var plinkoConfigService = new PlinkoConfigService(gameSettingsService);
            var plinkoPathFactory = new PlinkoPathFactory();
            var plinkoRuntimeService = new PlinkoRuntimeService();

            return new GameServicesContext
            {
                GameSettingsService = gameSettingsService,
                LocationConfigService = locationConfigService,
                LevelConfigService = levelConfigService,
                UnlocksService = unlocksService,
                WeightedRandomService = new WeightedRandomService(),
                UnitConfigService = unitConfigService,
                PinConfigService = pinConfigService,
                StatTypeConfigService = statTypeConfigService,
                PlinkoConfigService = plinkoConfigService,
                EnemyWaveSelectionService = new EnemyWaveSelectionService(),
                UnitNamingService = new UnitNamingService(unitNamesData),
                PlinkoPathFactory = plinkoPathFactory,
                BattleRuntimeService = new BattleRuntimeService(),
                PlinkoRuntimeService = plinkoRuntimeService,
                TrainingPipelineService = new TrainingPipelineService(
                    unitConfigService,
                    pinConfigService,
                    locationConfigService,
                    levelConfigService,
                    plinkoConfigService,
                    plinkoPathFactory,
                    plinkoRuntimeService),
                RunSaveService = new RunSaveService(Path.Combine(Application.persistentDataPath, "session_run_save.json")),
                MetaSaveService = metaSaveService,
                RunEntityIndex = new RunEntityIndex(),
                OwnedUnitIndex = new OwnedUnitIndex(),
                ShopOfferIndex = new ShopOfferIndex(),
                PinShopOfferIndex = new PinShopOfferIndex(),
                InstalledPinIndex = new InstalledPinIndex()
            };
        }
    }
}
