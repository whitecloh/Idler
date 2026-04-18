using Leopotam.EcsLite;
using Plinko.Scripts.Bootstrap;
using Plinko.Scripts.Data.Common;
using Plinko.Scripts.Models.ViewData;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Controllers;
using UnityEngine;

namespace Plinko.Scripts.View
{
    public sealed class UiCompositionRoot : MonoBehaviour
    {
        private enum ShellScreen
        {
            MainMenu = 0,
            LocationSelection = 1
        }

        [SerializeField] private MainMenuBridge mainMenuBridge;
        [SerializeField] private LocationBridge locationBridge;
        [SerializeField] private PurchasePhaseBridge purchasePhaseBridge;
        [SerializeField] private RetrainingPhaseBridge retrainingPhaseBridge;
        [SerializeField] private FieldUpgradeBridge fieldUpgradeBridge;
        [SerializeField] private BattleBridge battleBridge;
        [SerializeField] private MainMenuScreenController mainMenuScreenController;
        [SerializeField] private LocationSelectionScreenController locationSelectionScreenController;
        [SerializeField] private PurchasePhaseScreenController purchasePhaseScreenController;
        [SerializeField] private RetrainingPhaseScreenController retrainingPhaseScreenController;
        [SerializeField] private FieldUpgradePhaseScreenController fieldUpgradePhaseScreenController;
        [SerializeField] private BattleScreenController battleScreenController;
        [SerializeField] private BattleResultScreenController battleResultScreenController;
        [SerializeField] private OwnedUnitsScreenController ownedUnitsScreenController;
        [SerializeField] private UiWindowManager windowManager;

        private ShellScreen _shellScreen = ShellScreen.MainMenu;
        private bool _hadRunEntity;
        private bool _currentHasRunEntity;
        private Enums.PhaseType _currentPhase = Enums.PhaseType.MainMenu;

        public MainMenuScreenController MainMenuScreenController => mainMenuScreenController;
        public LocationSelectionScreenController LocationSelectionScreenController => locationSelectionScreenController;
        public PurchasePhaseScreenController PurchasePhaseScreenController => purchasePhaseScreenController;
        public RetrainingPhaseScreenController RetrainingPhaseScreenController => retrainingPhaseScreenController;
        public FieldUpgradePhaseScreenController FieldUpgradePhaseScreenController => fieldUpgradePhaseScreenController;
        public BattleScreenController BattleScreenController => battleScreenController;
        public BattleResultScreenController BattleResultScreenController => battleResultScreenController;
        public OwnedUnitsScreenController OwnedUnitsScreenController => ownedUnitsScreenController;

        private void Awake()
        {
            if (windowManager != null)
            {
                windowManager.Configure(
                    mainMenuScreenController,
                    purchasePhaseScreenController,
                    retrainingPhaseScreenController,
                    fieldUpgradePhaseScreenController,
                    battleScreenController,
                    battleResultScreenController);
                windowManager.ShowImmediate(UiWindowManager.WindowId.MainMenu);
            }
            else if (mainMenuScreenController != null)
            {
                mainMenuScreenController.SetVisibleImmediate(true);
            }

            if (locationSelectionScreenController != null)
            {
                locationSelectionScreenController.SetVisibleImmediate(false);
            }

            if (ownedUnitsScreenController != null)
            {
                ownedUnitsScreenController.SetVisibleImmediate(false);
            }
        }

        public void Configure(GameServicesContext services)
        {
        }

        public void Init(EcsWorld world)
        {
            if (mainMenuScreenController != null || locationSelectionScreenController != null)
            {
                mainMenuBridge.Init(world);
            }

            if (purchasePhaseScreenController != null ||
                retrainingPhaseScreenController != null ||
                fieldUpgradePhaseScreenController != null ||
                battleResultScreenController != null)
            {
                locationBridge.Init(world);
            }

            if (purchasePhaseScreenController != null)
            {
                purchasePhaseBridge.Init(world);
            }

            if (retrainingPhaseScreenController != null)
            {
                retrainingPhaseBridge.Init(world);
            }

            if (fieldUpgradePhaseScreenController != null)
            {
                fieldUpgradeBridge.Init(world);
            }

            if (battleScreenController != null || battleResultScreenController != null)
            {
                battleBridge.Init(world);
            }

            if (mainMenuScreenController != null)
            {
                mainMenuScreenController.Init(mainMenuBridge, ShowLocationSelection);
            }

            if (locationSelectionScreenController != null)
            {
                locationSelectionScreenController.Init(mainMenuBridge, ShowMainMenu);
            }

            if (purchasePhaseScreenController != null)
            {
                purchasePhaseScreenController.Init(purchasePhaseBridge, locationBridge);
            }

            if (retrainingPhaseScreenController != null)
            {
                retrainingPhaseScreenController.Init(retrainingPhaseBridge, locationBridge);
            }

            if (fieldUpgradePhaseScreenController != null)
            {
                fieldUpgradePhaseScreenController.Init(fieldUpgradeBridge, locationBridge);
            }

            if (battleScreenController != null)
            {
                battleScreenController.Init(battleBridge);
            }

            if (battleResultScreenController != null)
            {
                battleResultScreenController.Init(locationBridge, battleBridge);
            }

            ShowMainMenu();
            SyncScreenVisibility(false, Enums.PhaseType.MainMenu);
        }

        public void RefreshMainMenu(MainMenuViewData viewData)
        {
            if (mainMenuScreenController != null)
            {
                mainMenuScreenController.Refresh(viewData);
            }
        }

        public void RefreshLocationSelection(LocationSelectionViewData viewData)
        {
            if (locationSelectionScreenController != null)
            {
                locationSelectionScreenController.Refresh(viewData);
            }
        }

        public void RefreshPurchasePhase(PurchasePhaseViewData viewData)
        {
            if (purchasePhaseScreenController != null)
            {
                purchasePhaseScreenController.Refresh(viewData);
            }
        }

        public void RefreshRetrainingPhase(RetrainingPhaseViewData viewData)
        {
            if (retrainingPhaseScreenController != null)
            {
                retrainingPhaseScreenController.Refresh(viewData);
            }
        }

        public void RefreshFieldUpgradePhase(FieldUpgradePhaseViewData viewData)
        {
            if (fieldUpgradePhaseScreenController != null)
            {
                fieldUpgradePhaseScreenController.Refresh(viewData);
            }
        }

        public void RefreshOwnedUnits(System.Collections.Generic.IReadOnlyList<OwnedUnitViewData> ownedUnits)
        {
        }

        public void RefreshBattleHud(BattleHudViewData viewData)
        {
            if (battleScreenController != null)
            {
                battleScreenController.Refresh(viewData);
            }
        }

        public void RefreshBattleResult(BattleResultViewData viewData)
        {
            if (battleResultScreenController != null)
            {
                battleResultScreenController.Refresh(viewData);
            }
        }

        public void SyncScreenVisibility(bool hasRunEntity, Enums.PhaseType phase)
        {
            _currentHasRunEntity = hasRunEntity;
            _currentPhase = phase;

            if (!hasRunEntity && _hadRunEntity)
            {
                _shellScreen = ShellScreen.MainMenu;
            }

            _hadRunEntity = hasRunEntity;

            var showMainMenu = !hasRunEntity;
            var showLocationSelection = !hasRunEntity && _shellScreen == ShellScreen.LocationSelection;

            if (windowManager != null)
            {
                windowManager.Show(ResolvePrimaryWindow(hasRunEntity, phase));
            }
            else if (mainMenuScreenController != null)
            {
                mainMenuScreenController.Show(showMainMenu);
            }

            if (locationSelectionScreenController != null)
            {
                locationSelectionScreenController.Show(showLocationSelection);
            }

            if (windowManager == null)
            {
                if (purchasePhaseScreenController != null)
                {
                    purchasePhaseScreenController.Show(hasRunEntity && phase == Enums.PhaseType.PurchasePhase);
                }

                if (retrainingPhaseScreenController != null)
                {
                    retrainingPhaseScreenController.Show(hasRunEntity && phase == Enums.PhaseType.RetrainingPhase);
                }

                if (fieldUpgradePhaseScreenController != null)
                {
                    fieldUpgradePhaseScreenController.Show(hasRunEntity && phase == Enums.PhaseType.FieldUpgradePhase);
                }

                if (battleScreenController != null)
                {
                    battleScreenController.Show(hasRunEntity &&
                                                (phase == Enums.PhaseType.BattlePreparation ||
                                                 phase == Enums.PhaseType.Battle ||
                                                 phase == Enums.PhaseType.BattlePlayback));
                }

                if (battleResultScreenController != null)
                {
                    battleResultScreenController.Show(hasRunEntity && phase == Enums.PhaseType.Result);
                }
            }

            if (ownedUnitsScreenController != null)
            {
                ownedUnitsScreenController.Show(false);
            }
        }

        private void ShowMainMenu()
        {
            _shellScreen = ShellScreen.MainMenu;
            SyncScreenVisibility(_currentHasRunEntity, _currentPhase);
        }

        private void ShowLocationSelection()
        {
            _shellScreen = ShellScreen.LocationSelection;
            SyncScreenVisibility(_currentHasRunEntity, _currentPhase);
        }

        private static UiWindowManager.WindowId ResolvePrimaryWindow(bool hasRunEntity, Enums.PhaseType phase)
        {
            if (!hasRunEntity)
            {
                return UiWindowManager.WindowId.MainMenu;
            }

            return phase switch
            {
                Enums.PhaseType.PurchasePhase => UiWindowManager.WindowId.Purchase,
                Enums.PhaseType.RetrainingPhase => UiWindowManager.WindowId.Retraining,
                Enums.PhaseType.FieldUpgradePhase => UiWindowManager.WindowId.FieldUpgrade,
                Enums.PhaseType.BattlePreparation => UiWindowManager.WindowId.Battle,
                Enums.PhaseType.Battle => UiWindowManager.WindowId.Battle,
                Enums.PhaseType.BattlePlayback => UiWindowManager.WindowId.Battle,
                Enums.PhaseType.Result => UiWindowManager.WindowId.BattleResult,
                _ => UiWindowManager.WindowId.MainMenu
            };
        }
    }
}
