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
        [SerializeField] private SignalPurchaseBridge signalPurchaseBridge;
        [SerializeField] private RetrainingPhaseBridge retrainingPhaseBridge;
        [SerializeField] private FieldUpgradeBridge fieldUpgradeBridge;
        [SerializeField] private BattleBridge battleBridge;
        [SerializeField] private MainMenuScreenController mainMenuScreenController;
        [SerializeField] private LocationSelectionScreenController locationSelectionScreenController;
        [SerializeField] private PurchasePhaseScreenController purchasePhaseScreenController;
        [SerializeField] private SignalPurchasePhaseScreenController signalPurchasePhaseScreenController;
        [SerializeField] private RetrainingPhaseScreenController retrainingPhaseScreenController;
        [SerializeField] private FieldUpgradePhaseScreenController fieldUpgradePhaseScreenController;
        [SerializeField] private BattleScreenController standardBattleScreenController;
        [SerializeField] private BaseDefenseScreenController defenceBattleScreenController;
        [SerializeField] private PowerLineBattleScreenController powerLineBattleScreenController;
        [SerializeField] private BattleResultScreenController battleResultScreenController;
        [SerializeField] private OwnedUnitsScreenController ownedUnitsScreenController;
        [SerializeField] private UiWindowManager windowManager;

        private readonly UiPopupManager _popupManager = new();
        private ShellScreen _shellScreen = ShellScreen.MainMenu;
        private bool _hadRunEntity;
        private bool _currentHasRunEntity;
        private Enums.PhaseType _currentPhase = Enums.PhaseType.MainMenu;
        private Enums.LevelType _currentLevelType = Enums.LevelType.None;

        public MainMenuScreenController MainMenuScreenController => mainMenuScreenController;
        public LocationSelectionScreenController LocationSelectionScreenController => locationSelectionScreenController;
        public PurchasePhaseScreenController PurchasePhaseScreenController => purchasePhaseScreenController;
        public SignalPurchasePhaseScreenController SignalPurchasePhaseScreenController => signalPurchasePhaseScreenController;
        public RetrainingPhaseScreenController RetrainingPhaseScreenController => retrainingPhaseScreenController;
        public FieldUpgradePhaseScreenController FieldUpgradePhaseScreenController => fieldUpgradePhaseScreenController;
        public BattleScreenController BattleScreenController => standardBattleScreenController;
        public BaseDefenseScreenController DefenceBattleScreenController => defenceBattleScreenController;
        public PowerLineBattleScreenController PowerLineBattleScreenController => powerLineBattleScreenController;
        public BattleResultScreenController BattleResultScreenController => battleResultScreenController;
        public OwnedUnitsScreenController OwnedUnitsScreenController => ownedUnitsScreenController;

        private void Awake()
        {
            _popupManager.ClearRegistrations();
            RegisterPopupIfPresent(UiPopupManager.PopupId.LocationSelection, locationSelectionScreenController);

            if (windowManager != null)
            {
                windowManager.ClearRegistrations();
                RegisterWindowIfPresent(UiWindowManager.WindowId.MainMenu, mainMenuScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.Purchase, purchasePhaseScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.SignalPurchase, signalPurchasePhaseScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.Retraining, retrainingPhaseScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.FieldUpgrade, fieldUpgradePhaseScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.StandardBattle, standardBattleScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.DefenceBattle, defenceBattleScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.PowerLineBattle, powerLineBattleScreenController);
                RegisterWindowIfPresent(UiWindowManager.WindowId.BattleResult, battleResultScreenController);
                windowManager.OpenImmediate(UiWindowManager.WindowId.MainMenu);
            }
            else if (mainMenuScreenController != null)
            {
                mainMenuScreenController.SetVisibleImmediate(true);
            }

            _popupManager.CloseAll(true);

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
                signalPurchasePhaseScreenController != null ||
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

            if (signalPurchasePhaseScreenController != null)
            {
                signalPurchaseBridge.Init(world);
            }

            if (retrainingPhaseScreenController != null)
            {
                retrainingPhaseBridge.Init(world);
            }

            if (fieldUpgradePhaseScreenController != null)
            {
                fieldUpgradeBridge.Init(world);
            }

            if (standardBattleScreenController != null ||
                defenceBattleScreenController != null ||
                powerLineBattleScreenController != null ||
                battleResultScreenController != null)
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

            if (signalPurchasePhaseScreenController != null)
            {
                signalPurchasePhaseScreenController.Init(signalPurchaseBridge, locationBridge);
            }

            if (retrainingPhaseScreenController != null)
            {
                retrainingPhaseScreenController.Init(retrainingPhaseBridge, locationBridge);
            }

            if (fieldUpgradePhaseScreenController != null)
            {
                fieldUpgradePhaseScreenController.Init(fieldUpgradeBridge, locationBridge);
            }

            if (standardBattleScreenController != null)
            {
                standardBattleScreenController.Init(battleBridge);
            }

            if (defenceBattleScreenController != null)
            {
                defenceBattleScreenController.Init(battleBridge);
            }

            if (powerLineBattleScreenController != null)
            {
                powerLineBattleScreenController.Init(battleBridge);
            }

            if (battleResultScreenController != null)
            {
                battleResultScreenController.Init(locationBridge, battleBridge);
            }

            ShowMainMenu();
            SyncScreenVisibility(false, Enums.PhaseType.MainMenu, Enums.LevelType.None);
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

        public void RefreshSignalPurchasePhase(SignalPurchasePhaseViewData viewData)
        {
            if (signalPurchasePhaseScreenController != null)
            {
                signalPurchasePhaseScreenController.Refresh(viewData);
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

        public void RefreshStandardBattleHud(StandardBattleHudViewData viewData)
        {
            if (standardBattleScreenController != null)
            {
                standardBattleScreenController.Refresh(viewData);
            }
        }

        public void RefreshDefenceBattleHud(DefenceBattleHudViewData viewData)
        {
            if (defenceBattleScreenController != null)
            {
                defenceBattleScreenController.Refresh(viewData);
            }
        }

        public void RefreshPowerLineBattleHud(PowerLineBattleHudViewData viewData)
        {
            if (powerLineBattleScreenController != null)
            {
                powerLineBattleScreenController.Refresh(viewData);
            }
        }

        public void RefreshBattleResult(BattleResultViewData viewData)
        {
            if (battleResultScreenController != null)
            {
                battleResultScreenController.Refresh(viewData);
            }
        }

        public void SyncScreenVisibility(bool hasRunEntity, Enums.PhaseType phase, Enums.LevelType levelType)
        {
            _currentHasRunEntity = hasRunEntity;
            _currentPhase = phase;
            _currentLevelType = levelType;

            if (!hasRunEntity && _hadRunEntity)
            {
                _shellScreen = ShellScreen.MainMenu;
            }

            _hadRunEntity = hasRunEntity;

            var showMainMenu = !hasRunEntity;
            var showLocationSelection = !hasRunEntity && _shellScreen == ShellScreen.LocationSelection;

            if (windowManager != null)
            {
                windowManager.Open(ResolvePrimaryWindow(hasRunEntity, phase, levelType));
            }
            else if (mainMenuScreenController != null)
            {
                mainMenuScreenController.Show(showMainMenu);
            }

            if (showLocationSelection)
            {
                _popupManager.Open(UiPopupManager.PopupId.LocationSelection);
            }
            else
            {
                _popupManager.Close(UiPopupManager.PopupId.LocationSelection);
            }

            if (windowManager == null)
            {
                if (purchasePhaseScreenController != null)
                {
                    purchasePhaseScreenController.Show(hasRunEntity && phase == Enums.PhaseType.PurchasePhase);
                }

                if (signalPurchasePhaseScreenController != null)
                {
                    signalPurchasePhaseScreenController.Show(hasRunEntity && phase == Enums.PhaseType.SignalPurchasePhase);
                }

                if (retrainingPhaseScreenController != null)
                {
                    retrainingPhaseScreenController.Show(hasRunEntity && phase == Enums.PhaseType.RetrainingPhase);
                }

                if (fieldUpgradePhaseScreenController != null)
                {
                    fieldUpgradePhaseScreenController.Show(hasRunEntity && phase == Enums.PhaseType.FieldUpgradePhase);
                }

                if (standardBattleScreenController != null)
                {
                    standardBattleScreenController.Show(hasRunEntity &&
                                                        levelType == Enums.LevelType.StandardBattle &&
                                                        (phase == Enums.PhaseType.BattlePreparation ||
                                                         phase == Enums.PhaseType.Battle ||
                                                         phase == Enums.PhaseType.BattlePlayback));
                }

                if (defenceBattleScreenController != null)
                {
                    defenceBattleScreenController.Show(hasRunEntity &&
                                                       levelType == Enums.LevelType.DefenceBattle &&
                                                       (phase == Enums.PhaseType.BattlePreparation ||
                                                        phase == Enums.PhaseType.Battle ||
                                                        phase == Enums.PhaseType.BattlePlayback));
                }

                if (powerLineBattleScreenController != null)
                {
                    powerLineBattleScreenController.Show(hasRunEntity &&
                                                         levelType == Enums.LevelType.PowerLineBattle &&
                                                         phase == Enums.PhaseType.Battle);
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
            SyncScreenVisibility(_currentHasRunEntity, _currentPhase, _currentLevelType);
        }

        private void ShowLocationSelection()
        {
            _shellScreen = ShellScreen.LocationSelection;
            SyncScreenVisibility(_currentHasRunEntity, _currentPhase, _currentLevelType);
        }

        private void RegisterWindowIfPresent(UiWindowManager.WindowId id, IUiWindow window)
        {
            if (window == null)
            {
                return;
            }

            windowManager.Register(id, window);
        }

        private void RegisterPopupIfPresent(UiPopupManager.PopupId id, IUiWindow popup)
        {
            if (popup == null)
            {
                return;
            }

            _popupManager.Register(id, popup);
        }

        private static UiWindowManager.WindowId ResolvePrimaryWindow(bool hasRunEntity, Enums.PhaseType phase, Enums.LevelType levelType)
        {
            if (!hasRunEntity)
            {
                return UiWindowManager.WindowId.MainMenu;
            }

            return phase switch
            {
                Enums.PhaseType.PurchasePhase => UiWindowManager.WindowId.Purchase,
                Enums.PhaseType.SignalPurchasePhase => UiWindowManager.WindowId.SignalPurchase,
                Enums.PhaseType.RetrainingPhase => UiWindowManager.WindowId.Retraining,
                Enums.PhaseType.FieldUpgradePhase => UiWindowManager.WindowId.FieldUpgrade,
                Enums.PhaseType.BattlePreparation => levelType == Enums.LevelType.DefenceBattle ? UiWindowManager.WindowId.DefenceBattle : UiWindowManager.WindowId.StandardBattle,
                Enums.PhaseType.Battle => levelType switch
                {
                    Enums.LevelType.DefenceBattle => UiWindowManager.WindowId.DefenceBattle,
                    Enums.LevelType.PowerLineBattle => UiWindowManager.WindowId.PowerLineBattle,
                    _ => UiWindowManager.WindowId.StandardBattle
                },
                Enums.PhaseType.BattlePlayback => levelType == Enums.LevelType.DefenceBattle ? UiWindowManager.WindowId.DefenceBattle : UiWindowManager.WindowId.StandardBattle,
                Enums.PhaseType.Result => UiWindowManager.WindowId.BattleResult,
                _ => UiWindowManager.WindowId.MainMenu
            };
        }
    }
}
