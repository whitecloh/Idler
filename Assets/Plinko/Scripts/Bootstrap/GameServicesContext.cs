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
        public UnlocksService UnlocksService;
        public WeightedRandomService WeightedRandomService;
        public UnitConfigService UnitConfigService;
        public PinConfigService PinConfigService;
        public PlinkoConfigService PlinkoConfigService;
        public EnemyWaveSelectionService EnemyWaveSelectionService;
        public UnitNamingService UnitNamingService;
        public PlinkoPathFactory PlinkoPathFactory;
        public BattleRuntimeService BattleRuntimeService;
        public PlinkoRuntimeService PlinkoRuntimeService;
        public TrainingPipelineService TrainingPipelineService;
        public RunSaveService RunSaveService;
        public MetaSaveService MetaSaveService;
        public RunEntityIndex RunEntityIndex;
        public OwnedUnitIndex OwnedUnitIndex;
        public ShopOfferIndex ShopOfferIndex;
        public PinShopOfferIndex PinShopOfferIndex;
        public InstalledPinIndex InstalledPinIndex;
    }
}
