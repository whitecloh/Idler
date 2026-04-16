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

        private ShellScreen _shellScreen = ShellScreen.MainMenu;
        private bool _hadRunEntity;

        public MainMenuScreenController MainMenuScreenController => mainMenuScreenController;
        public LocationSelectionScreenController LocationSelectionScreenController => locationSelectionScreenController;
        public PurchasePhaseScreenController PurchasePhaseScreenController => purchasePhaseScreenController;
        public RetrainingPhaseScreenController RetrainingPhaseScreenController => retrainingPhaseScreenController;
        public FieldUpgradePhaseScreenController FieldUpgradePhaseScreenController => fieldUpgradePhaseScreenController;
        public BattleScreenController BattleScreenController => battleScreenController;
        public BattleResultScreenController BattleResultScreenController => battleResultScreenController;
        public OwnedUnitsScreenController OwnedUnitsScreenController => ownedUnitsScreenController;

        public void Configure(GameServicesContext services)
        {
            EnsureRuntimeWiring();
        }

        public void Init(EcsWorld world)
        {
            EnsureRuntimeWiring();
            if (mainMenuBridge != null) mainMenuBridge.Init(world);
            if (locationBridge != null) locationBridge.Init(world);
            if (purchasePhaseBridge != null) purchasePhaseBridge.Init(world);
            if (retrainingPhaseBridge != null) retrainingPhaseBridge.Init(world);
            if (fieldUpgradeBridge != null) fieldUpgradeBridge.Init(world);
            if (battleBridge != null) battleBridge.Init(world);

            mainMenuScreenController?.Init(mainMenuBridge, ShowLocationSelection);
            locationSelectionScreenController?.Init(mainMenuBridge, ShowMainMenu);
            ShowMainMenu();
        }

        public void RefreshMainMenu(MainMenuViewData viewData)
        {
            mainMenuScreenController?.Refresh(viewData);
        }

        public void RefreshLocationSelection(LocationSelectionViewData viewData)
        {
            locationSelectionScreenController?.Refresh(viewData);
        }

        public void SyncScreenVisibility(bool hasRunEntity, Enums.PhaseType phase)
        {
            if (!hasRunEntity && _hadRunEntity)
            {
                _shellScreen = ShellScreen.MainMenu;
            }

            _hadRunEntity = hasRunEntity;

            var showMainMenu = !hasRunEntity && _shellScreen == ShellScreen.MainMenu;
            var showLocationSelection = !hasRunEntity && _shellScreen == ShellScreen.LocationSelection;
            mainMenuScreenController?.Show(showMainMenu);
            locationSelectionScreenController?.Show(showLocationSelection);

            purchasePhaseScreenController?.Show(hasRunEntity && phase == Enums.PhaseType.PurchasePhase);
            retrainingPhaseScreenController?.Show(hasRunEntity && phase == Enums.PhaseType.RetrainingPhase);
            fieldUpgradePhaseScreenController?.Show(hasRunEntity && phase == Enums.PhaseType.FieldUpgradePhase);
            battleScreenController?.Show(hasRunEntity &&
                                         (phase == Enums.PhaseType.BattlePreparation ||
                                          phase == Enums.PhaseType.Battle ||
                                          phase == Enums.PhaseType.BattlePlayback));
            battleResultScreenController?.Show(hasRunEntity && phase == Enums.PhaseType.Result);
            ownedUnitsScreenController?.Show(hasRunEntity &&
                                             (phase == Enums.PhaseType.PurchasePhase ||
                                              phase == Enums.PhaseType.RetrainingPhase ||
                                              phase == Enums.PhaseType.FieldUpgradePhase ||
                                              phase == Enums.PhaseType.BattlePreparation));
        }

        private void ShowMainMenu()
        {
            _shellScreen = ShellScreen.MainMenu;
        }

        private void ShowLocationSelection()
        {
            _shellScreen = ShellScreen.LocationSelection;
        }

        private void EnsureRuntimeWiring()
        {
            mainMenuBridge ??= GetComponent<MainMenuBridge>() ?? gameObject.AddComponent<MainMenuBridge>();
            locationBridge ??= GetComponent<LocationBridge>() ?? gameObject.AddComponent<LocationBridge>();
            purchasePhaseBridge ??= GetComponent<PurchasePhaseBridge>() ?? gameObject.AddComponent<PurchasePhaseBridge>();
            retrainingPhaseBridge ??= GetComponent<RetrainingPhaseBridge>() ?? gameObject.AddComponent<RetrainingPhaseBridge>();
            fieldUpgradeBridge ??= GetComponent<FieldUpgradeBridge>() ?? gameObject.AddComponent<FieldUpgradeBridge>();
            battleBridge ??= GetComponent<BattleBridge>() ?? gameObject.AddComponent<BattleBridge>();
            mainMenuScreenController ??= GetComponent<MainMenuScreenController>() ?? gameObject.AddComponent<MainMenuScreenController>();
            locationSelectionScreenController ??= GetComponent<LocationSelectionScreenController>() ?? gameObject.AddComponent<LocationSelectionScreenController>();
            purchasePhaseScreenController ??= GetComponent<PurchasePhaseScreenController>() ?? gameObject.AddComponent<PurchasePhaseScreenController>();
            retrainingPhaseScreenController ??= GetComponent<RetrainingPhaseScreenController>() ?? gameObject.AddComponent<RetrainingPhaseScreenController>();
            fieldUpgradePhaseScreenController ??= GetComponent<FieldUpgradePhaseScreenController>() ?? gameObject.AddComponent<FieldUpgradePhaseScreenController>();
            battleScreenController ??= GetComponent<BattleScreenController>() ?? gameObject.AddComponent<BattleScreenController>();
            battleResultScreenController ??= GetComponent<BattleResultScreenController>() ?? gameObject.AddComponent<BattleResultScreenController>();
            ownedUnitsScreenController ??= GetComponent<OwnedUnitsScreenController>() ?? gameObject.AddComponent<OwnedUnitsScreenController>();
        }
    }
}
