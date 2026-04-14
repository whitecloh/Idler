using Leopotam.EcsLite;
using Plinko.Scripts.Bootstrap;
using Plinko.Scripts.ECS.Systems;
using Plinko.Scripts.ECS.UISystems;
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
                .Add(new StartNewRunSystem(_services.GameSettingsService, _services.RunEntityIndex))
                .Add(new ContinueRunSystem(_services.RunSaveService, _services.RunEntityIndex))
                .Add(new LoadLevelSystem(_services.LevelConfigService, _services.RunEntityIndex))
                .Add(new RouteLevelTypeToPhaseSystem(_services.RunEntityIndex))
                .Add(new EnterPurchasePhaseSystem(_services.RunEntityIndex, _services.GameSettingsService))
                .Add(new EnterUpgradePhaseSystem(_services.RunEntityIndex))
                .Add(new EnterFieldUpgradePhaseSystem(_services.RunEntityIndex, _services.GameSettingsService))
                .Add(new InitializePlinkoBoardSystem(_services.GameSettingsService))
                .Add(new RerollUnitShopSystem(_services.RunEntityIndex, _services.GameSettingsService))
                .Add(new GenerateUnitShopOffersSystem(_services.UnitConfigService, _services.ShopOfferIndex))
                .Add(new RerollPinShopSystem(_services.RunEntityIndex, _services.GameSettingsService))
                .Add(new GeneratePinShopOffersSystem(_services.PinConfigService, _services.PinShopOfferIndex))
                .Add(new BuyUnitSystem(_services.UnitConfigService, _services.RunEntityIndex, _services.ShopOfferIndex))
                .Add(new BuyPinSystem(_services.PinConfigService, _services.RunEntityIndex, _services.PinShopOfferIndex))
                .Add(new SelectUnitsForUpgradeSystem(_services.RunEntityIndex, _services.OwnedUnitIndex, _services.GameSettingsService))
                .Add(new GenerateHandSystem(_services.RunEntityIndex, _services.GameSettingsService))
                .Add(new ClearHandSystem(_services.RunEntityIndex))
                .Add(new DeployUnitSystem(_services.RunEntityIndex))
                .Add(new PrepareEnemyTurnSystem(_services.RunEntityIndex))
                .Add(new ResolveBattleSystem(_services.RunEntityIndex, _services.BattleRuntimeService))
                .Add(new StartBattlePlaybackSystem(_services.RunEntityIndex))
                .Add(new RouteBattleOutcomeAfterPlaybackSystem(_services.RunEntityIndex))
                .Add(new AdvanceToNextLevelSystem(_services.RunEntityIndex, _services.LocationConfigService, _services.BattleRuntimeService))
                .Add(new ReturnToMenuSystem(_services.RunEntityIndex, _services.BattleRuntimeService))
                .Add(new SelectBoardSlotSystem(_services.RunEntityIndex, _services.GameSettingsService))
                .Add(new ValidateUpgradeSelectionCountSystem(_services.RunEntityIndex, _services.GameSettingsService))
                .Add(new ReplaceBoardPinSystem(_services.RunEntityIndex))
                .Add(new StageSelectedUnitsForUpgradeSystem())
                .Add(new StartTrainingSystem())
                .Add(new FinalizePurchasedTrainingResultsSystem(_services.UnitConfigService, _services.UnitNamingService))
                .Add(new FinalizeUpgradedUnitReplacementSystem())
                .Add(new RegisterOwnedUnitSystem(_services.OwnedUnitIndex))
                .Add(new RemoveOwnedUnitSystem(_services.OwnedUnitIndex))
                .Add(new ReplaceOwnedUnitVersionSystem(_services.OwnedUnitIndex))
                .Add(new ResetUpgradeSelectionStateAfterTrainingSystem(_services.RunEntityIndex))
                .Add(new WriteRunSaveSystem(_services.RunSaveService, _services.RunEntityIndex))
                .Add(new RefreshPurchasePhaseUiSystem(_uiCompositionRoot != null ? _uiCompositionRoot.PurchasePhaseScreenController : null, _services.UnitConfigService, _services.GameSettingsService, _services.RunEntityIndex))
                .Add(new RefreshUpgradePhaseUiSystem(_uiCompositionRoot != null ? _uiCompositionRoot.UpgradePhaseScreenController : null, _services.RunEntityIndex, _services.GameSettingsService))
                .Add(new RefreshFieldUpgradeUiSystem(_uiCompositionRoot != null ? _uiCompositionRoot.FieldUpgradePhaseScreenController : null, _services.PinConfigService, _services.GameSettingsService, _services.RunEntityIndex))
                .Add(new RefreshBattleHudUiSystem(_uiCompositionRoot != null ? _uiCompositionRoot.BattleScreenController : null, _services.RunEntityIndex))
                .Add(new RefreshBattleResultUiSystem(_uiCompositionRoot != null ? _uiCompositionRoot.BattleResultScreenController : null, _services.RunEntityIndex, _services.BattleRuntimeService))
                .Add(new RefreshOwnedUnitsUiSystem(_uiCompositionRoot != null ? _uiCompositionRoot.OwnedUnitsScreenController : null))
                .Add(new CleanupTransientEventsSystem());
        }
    }
}