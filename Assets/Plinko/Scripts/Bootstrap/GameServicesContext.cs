using System;
using Plinko.Scripts.ECS.Indexes;
using Plinko.Scripts.Services;

namespace Plinko.Scripts.Bootstrap
{
    [Serializable]
    public sealed class GameServicesContext
    {
        public GameSettingsService GameSettingsService;
        public LocationConfigService LocationConfigService;
        public LevelConfigService LevelConfigService;
        public UnitConfigService UnitConfigService;
        public PinConfigService PinConfigService;
        public UnitNamingService UnitNamingService;
        public BattleRuntimeService BattleRuntimeService;
        public RunSaveService RunSaveService;
        public RunEntityIndex RunEntityIndex;
        public OwnedUnitIndex OwnedUnitIndex;
        public ShopOfferIndex ShopOfferIndex;
        public PinShopOfferIndex PinShopOfferIndex;
    }
}