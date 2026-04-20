using Leopotam.EcsLite;
using Plinko.Scripts.Bootstrap;
using Plinko.Scripts.ECS.Systems;
using Plinko.Scripts.ECS.Systems.UISystems;
using Plinko.Scripts.View;

namespace Plinko.Scripts.ECS.Installers
{
    public sealed class EcsCompositionRoot
    {
        private readonly GameServicesContext _services;
        private readonly UiCompositionRoot _uiCompositionRoot;

        public EcsCompositionRoot(GameServicesContext services, UiCompositionRoot uiCompositionRoot)
        {
            _services = services;
            _uiCompositionRoot = uiCompositionRoot;
        }

        public IEcsSystems Create(EcsWorld world)
        {
            return new EcsSystems(world)
                .Add(new StartNewRunSystem(
                    _services.LocationConfigService,
                    _services.UnlocksService,
                    _services.GameSettingsService,
                    _services.PlinkoRuntimeService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex,
                    _services.ShopOfferIndex,
                    _services.PinShopOfferIndex,
                    _services.InstalledPinIndex))
                .Add(new ContinueRunSystem(
                    _services.RunSaveService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.UnitConfigService,
                    _services.GameSettingsService,
                    _services.PlinkoRuntimeService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex,
                    _services.ShopOfferIndex,
                    _services.PinShopOfferIndex,
                    _services.InstalledPinIndex))
                .Add(new RestoreRunBoardSystem(
                    _services.LocationConfigService,
                    _services.RunEntityIndex,
                    _services.InstalledPinIndex))
                .Add(new RestoreOwnedUnitsSystem(_services.OwnedUnitIndex))
                .Add(new LoadLevelSystem(
                    _services.LevelConfigService,
                    _services.GameSettingsService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new RouteLevelTypeToPhaseSystem(
                    _services.LevelConfigService,
                    _services.GameSettingsService,
                    _services.RunEntityIndex))
                .Add(new InitializeLocationBoardSystem(
                    _services.LocationConfigService,
                    _services.RunEntityIndex,
                    _services.InstalledPinIndex))
                .Add(new GenerateUnitShopOffersSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.UnitConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.ShopOfferIndex))
                .Add(new GenerateSignalUnitShopOffersSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.UnitConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.ShopOfferIndex))
                .Add(new RerollUnitShopSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.UnitConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.ShopOfferIndex))
                .Add(new RerollSignalUnitShopSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.UnitConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.ShopOfferIndex))
                .Add(new BuyUnitSystem(
                    _services.UnitConfigService,
                    _services.UnitNamingService,
                    _services.LevelConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.ShopOfferIndex))
                .Add(new BuySignalUnitSystem(
                    _services.UnitConfigService,
                    _services.UnitNamingService,
                    _services.LevelConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.ShopOfferIndex))
                .Add(new BeginPurchasedTrainingSystem(
                    _services.TrainingPipelineService,
                    _services.RunEntityIndex))
                .Add(new LaunchSignalSystem(
                    _services.TrainingPipelineService,
                    _services.RunEntityIndex))
                .Add(new AdvancePlinkoTrainingPlaybackSystem())
                .Add(new CompletePurchasedTrainingSystem(
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex))
                .Add(new CompleteSignalPurchaseTrainingSystem(
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex))
                .Add(new AccrueSignalPurchaseIncomeSystem(
                    _services.GameSettingsService,
                    _services.UnitConfigService,
                    _services.RunEntityIndex))
                .Add(new GenerateRetrainingShopOffersSystem(
                    _services.UnitConfigService,
                    _services.RunEntityIndex))
                .Add(new RerollRetrainingShopSystem(
                    _services.GameSettingsService,
                    _services.UnitConfigService,
                    _services.RunEntityIndex))
                .Add(new BuyRetrainingBatchSystem(
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex))
                .Add(new BeginRetrainingSystem(
                    _services.TrainingPipelineService,
                    _services.UnitConfigService,
                    _services.RunEntityIndex))
                .Add(new CompleteRetrainingSystem(
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex))
                .Add(new GeneratePinShopOffersSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.PinConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.PinShopOfferIndex))
                .Add(new RerollPinShopSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.PinConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.PinShopOfferIndex))
                .Add(new BuyPinSystem(
                    _services.LevelConfigService,
                    _services.PinConfigService,
                    _services.WeightedRandomService,
                    _services.RunEntityIndex,
                    _services.PinShopOfferIndex))
                .Add(new SelectBoardSlotSystem(
                    _services.RunEntityIndex,
                    _services.InstalledPinIndex))
                .Add(new ReplaceBoardPinSystem(
                    _services.RunEntityIndex,
                    _services.InstalledPinIndex))
                .Add(new SelectStandardEnemyWaveSystem(
                    _services.EnemyWaveSelectionService,
                    _services.LevelConfigService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new StartDefenceBattleSystem(
                    _services.RunEntityIndex))
                .Add(new ResolveBattleSystem(
                    _services.BattleRuntimeService,
                    _services.GameSettingsService,
                    _services.OwnedUnitIndex,
                    _services.RunEntityIndex))
                .Add(new ResolveBaseDefenseBattleSystem(
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new StartBattlePlaybackSystem(
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new RouteBattleOutcomeAfterPlaybackSystem(
                    _services.BattleRuntimeService,
                    _services.LevelConfigService,
                    _services.LocationConfigService,
                    _services.RunEntityIndex))
                .Add(new BeginStandardBattleTurnSystem(
                    _services.GameSettingsService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new BeginDefenceBattleTurnSystem(
                    _services.LevelConfigService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new InitializePowerLineBattleSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new DrawPowerLineHandCardsSystem(
                    _services.RunEntityIndex))
                .Add(new RerollPowerLineHandSystem(
                    _services.GameSettingsService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new GenerateHandSystem(
                    _services.GameSettingsService,
                    _services.RunEntityIndex))
                .Add(new ClearHandSystem(_services.RunEntityIndex))
                .Add(new DeployStandardBattleCardSystem(
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex))
                .Add(new DeployDefenceBattleCardSystem(
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex,
                    _services.BattleRuntimeService,
                    _services.UnitConfigService))
                .Add(new DeployPowerLineBattleCardSystem(
                    _services.BattleRuntimeService,
                    _services.UnitConfigService,
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex))
                .Add(new TickPowerLineBattleSystem(
                    _services.GameSettingsService,
                    _services.LevelConfigService,
                    _services.LocationConfigService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new ClampGoldSystem(
                    _services.GameSettingsService,
                    _services.RunEntityIndex))
                .Add(new AdvanceToNextLevelSystem(
                    _services.BattleRuntimeService,
                    _services.LevelConfigService,
                    _services.RunEntityIndex))
                .Add(new PersistMetaProgressSystem(
                    _services.UnlocksService,
                    _services.MetaSaveService,
                    _services.RunEntityIndex))
                .Add(new ReturnToMenuSystem(
                    _services.RunSaveService,
                    _services.PlinkoRuntimeService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex,
                    _services.ShopOfferIndex,
                    _services.PinShopOfferIndex,
                    _services.InstalledPinIndex))
                .Add(new RegisterOwnedUnitSystem(_services.OwnedUnitIndex))
                .Add(new ReplaceOwnedUnitSystem(_services.OwnedUnitIndex))
                .Add(new WriteRunSaveSystem(
                    _services.RunSaveService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex))
                .Add(new RefreshMenuLocationUiSystem(
                    _services.RunSaveService,
                    _services.LocationConfigService,
                    _services.UnlocksService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshPurchasePhaseUiSystem(
                    _services.GameSettingsService,
                    _services.UnitConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.PlinkoConfigService,
                    _services.PinConfigService,
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshSignalPurchasePhaseUiSystem(
                    _services.GameSettingsService,
                    _services.UnitConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.PlinkoConfigService,
                    _services.PinConfigService,
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshRetrainingWindowUiSystem(
                    _services.GameSettingsService,
                    _services.UnitConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.PlinkoConfigService,
                    _services.PinConfigService,
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshFieldUpgradeUiSystem(
                    _services.GameSettingsService,
                    _services.PinConfigService,
                    _services.UnitConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.PlinkoConfigService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshOwnedUnitsUiSystem(
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshStandardBattleHudUiSystem(
                    _services.GameSettingsService,
                    _services.UnitConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.EnemyWaveSelectionService,
                    _services.BattleRuntimeService,
                    _services.OwnedUnitIndex,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshDefenceBattleHudUiSystem(
                    _services.UnitConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshPowerLineBattleHudUiSystem(
                    _services.UnitConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.BattleRuntimeService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new RefreshBattleResultUiSystem(
                    _services.BattleRuntimeService,
                    _services.LocationConfigService,
                    _services.RunEntityIndex,
                    _uiCompositionRoot))
                .Add(new CleanupTransientEventsSystem());
        }
    }
}
