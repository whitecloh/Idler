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
            var locationConfigService = new LocationConfigService(locations);
            var unitConfigService = new UnitConfigService(unitTypes);
            var pinConfigService = new PinConfigService(pinTypes);
            var savePath = Path.Combine(Application.persistentDataPath, "session_run_save.json");

            return new GameServicesContext
            {
                GameSettingsService = new GameSettingsService(gameSettingsData),
                LocationConfigService = locationConfigService,
                LevelConfigService = new LevelConfigService(locationConfigService),
                UnitConfigService = unitConfigService,
                PinConfigService = pinConfigService,
                UnitNamingService = new UnitNamingService(unitNamesData),
                BattleRuntimeService = new BattleRuntimeService(),
                RunSaveService = new RunSaveService(savePath),
                RunEntityIndex = new RunEntityIndex(),
                OwnedUnitIndex = new OwnedUnitIndex(),
                ShopOfferIndex = new ShopOfferIndex(),
                PinShopOfferIndex = new PinShopOfferIndex()
            };
        }
    }
}