using Leopotam.EcsLite;
using Plinko.Scripts.View.Bridges;
using Plinko.Scripts.View.Controllers;
using UnityEngine;

namespace Plinko.Scripts.View
{
    public sealed class UiCompositionRoot : MonoBehaviour
    {
        [SerializeField] private MainMenuBridge mainMenuBridge;
        [SerializeField] private LocationBridge locationBridge;
        [SerializeField] private PurchasePhaseBridge purchasePhaseBridge;
        [SerializeField] private UpgradePhaseBridge upgradePhaseBridge;
        [SerializeField] private FieldUpgradePhaseBridge fieldUpgradePhaseBridge;
        [SerializeField] private BattleBridge battleBridge;
        [SerializeField] private PurchasePhaseScreenController purchasePhaseScreenController;
        [SerializeField] private UpgradePhaseScreenController upgradePhaseScreenController;
        [SerializeField] private FieldUpgradePhaseScreenController fieldUpgradePhaseScreenController;
        [SerializeField] private BattleScreenController battleScreenController;
        [SerializeField] private BattleResultScreenController battleResultScreenController;
        [SerializeField] private OwnedUnitsScreenController ownedUnitsScreenController;

        public PurchasePhaseScreenController PurchasePhaseScreenController => purchasePhaseScreenController;
        public UpgradePhaseScreenController UpgradePhaseScreenController => upgradePhaseScreenController;
        public FieldUpgradePhaseScreenController FieldUpgradePhaseScreenController => fieldUpgradePhaseScreenController;
        public BattleScreenController BattleScreenController => battleScreenController;
        public BattleResultScreenController BattleResultScreenController => battleResultScreenController;
        public OwnedUnitsScreenController OwnedUnitsScreenController => ownedUnitsScreenController;
        
        public void Init(EcsWorld world)
        {
            if (mainMenuBridge != null)
            {
                mainMenuBridge.Init(world);
            }

            if (locationBridge != null)
            {
                locationBridge.Init(world);
            }

            if (purchasePhaseBridge != null)
            {
                purchasePhaseBridge.Init(world);
            }

            if (upgradePhaseBridge != null)
            {
                upgradePhaseBridge.Init(world);
            }

            if (fieldUpgradePhaseBridge != null)
            {
                fieldUpgradePhaseBridge.Init(world);
            }

            if (battleBridge != null)
            {
                battleBridge.Init(world);
            }
        }
    }
}