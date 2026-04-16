using System.Collections.Generic;
using System.IO;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.Data.Pins;
using Plinko.Scripts.Data.Settings;
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

        public GameServicesContext Build()
        {
            var unlocksService = new UnlocksService();
            var gameSettingsService = new GameSettingsService(gameSettingsData);
            var locationConfigService = new LocationConfigService(locations);

            return new GameServicesContext
            {
                GameSettingsService = gameSettingsService,
                LocationConfigService = locationConfigService,
                LevelConfigService = new LevelConfigService(locationConfigService),
                UnlocksService = unlocksService,
                WeightedRandomService = new WeightedRandomService(),
                UnitConfigService = new UnitConfigService(unitTypes, unlocksService),
                PinConfigService = new PinConfigService(pinTypes, unlocksService),
                PlinkoConfigService = new PlinkoConfigService(gameSettingsService),
                EnemyWaveSelectionService = new EnemyWaveSelectionService(),
                UnitNamingService = new UnitNamingService(unitNamesData),
                PlinkoPathFactory = new PlinkoPathFactory(),
                BattleRuntimeService = new BattleRuntimeService(),
                PlinkoRuntimeService = new PlinkoRuntimeService(),
                RunSaveService = new RunSaveService(Path.Combine(Application.persistentDataPath, "session_run_save.json")),
                RunEntityIndex = new RunEntityIndex(),
                OwnedUnitIndex = new OwnedUnitIndex(),
                ShopOfferIndex = new ShopOfferIndex(),
                PinShopOfferIndex = new PinShopOfferIndex(),
                InstalledPinIndex = new InstalledPinIndex()
            };
        }
    }
}