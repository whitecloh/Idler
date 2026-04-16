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
                    _services.GameSettingsService,
                    _services.RunEntityIndex,
                    _services.OwnedUnitIndex,
                    _services.ShopOfferIndex,
                    _services.PinShopOfferIndex,
                    _services.InstalledPinIndex))
                .Add(new ContinueRunSystem(
                    _services.RunSaveService,
                    _services.GameSettingsService,
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
                .Add(new LoadLevelSystem(_services.LevelConfigService, _services.RunEntityIndex))
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
                .Add(new RerollUnitShopSystem(
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
                .Add(new BeginPurchasedTrainingSystem(
                    _services.UnitConfigService,
                    _services.PinConfigService,
                    _services.LocationConfigService,
                    _services.LevelConfigService,
                    _services.PlinkoConfigService,
                    _services.PlinkoPathFactory,
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex))
                .Add(new AdvancePlinkoTrainingPlaybackSystem())
                .Add(new CompletePurchasedTrainingSystem(
                    _services.PlinkoRuntimeService,
                    _services.RunEntityIndex))
                .Add(new SelectUnitsForRetrainingSystem())
                .Add(new ConfirmRetrainingSelectionSystem())
                .Add(new BeginRetrainingSystem())
                .Add(new CompleteRetrainingSystem())
                .Add(new GeneratePinShopOffersSystem())
                .Add(new RerollPinShopSystem())
                .Add(new BuyPinSystem())
                .Add(new SelectBoardSlotSystem())
                .Add(new ReplaceBoardPinSystem())
                .Add(new GenerateHandSystem())
                .Add(new ClearHandSystem())
                .Add(new DeployCardSystem())
                .Add(new SelectEnemyWaveSystem())
                .Add(new ResolveBattleSystem())
                .Add(new StartBattlePlaybackSystem())
                .Add(new RouteBattleOutcomeAfterPlaybackSystem())
                .Add(new AdvanceToNextLevelSystem())
                .Add(new ReturnToMenuSystem())
                .Add(new RegisterOwnedUnitSystem(_services.OwnedUnitIndex))
                .Add(new ReplaceOwnedUnitSystem(_services.OwnedUnitIndex))
                .Add(new WriteRunSaveSystem(_services.RunSaveService, _services.RunEntityIndex))
                .Add(new RefreshPurchasePhaseUiSystem())
                .Add(new RefreshRetrainingPhaseUiSystem())
                .Add(new RefreshFieldUpgradeUiSystem())
                .Add(new RefreshOwnedUnitsUiSystem())
                .Add(new RefreshBattleHudUiSystem())
                .Add(new RefreshBattleResultUiSystem())
                .Add(new CleanupTransientEventsSystem());
        }
    }
}